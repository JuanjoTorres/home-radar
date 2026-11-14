using Amazon.CDK;
using Amazon.CDK.AWS.Budgets;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.S3;
using Constructs;

namespace HomeRadar.Cdk;

public sealed class HomeRadarStackProps : StackProps
{
    public string BudgetEmail { get; init; } = string.Empty;

    public double MonthlyBudgetUsd { get; init; } = 15;
}

/// <summary>
/// Pay-per-use AWS pieces only (ADR-0002): one S3 bucket for nightly database dumps and photo copies with a lifecycle that
/// moves old objects to cheaper classes, one least-privilege IAM user for the VPS, and a cost budget with e-mail alerts.
/// Compute runs on a VPS and is deployed with Docker Compose, not with CDK.
/// </summary>
public sealed class HomeRadarStack : Stack
{
    public HomeRadarStack(Construct scope, string id, HomeRadarStackProps props)
        : base(scope, id, props)
    {
        var bucket = new Bucket(this, "Storage", new BucketProps
        {
            BucketName = $"homeradar-{Account}-{Region}",
            BlockPublicAccess = BlockPublicAccess.BLOCK_ALL,
            Encryption = BucketEncryption.S3_MANAGED,
            EnforceSSL = true,
            Versioned = false,
            RemovalPolicy = RemovalPolicy.RETAIN,
            LifecycleRules =
            [
                new LifecycleRule
                {
                    Id = "backups",
                    Prefix = "backups/",
                    Transitions =
                    [
                        new Transition { StorageClass = StorageClass.INFREQUENT_ACCESS, TransitionAfter = Duration.Days(30) },
                        new Transition { StorageClass = StorageClass.DEEP_ARCHIVE, TransitionAfter = Duration.Days(180) },
                    ],
                    Expiration = Duration.Days(730),
                },
                new LifecycleRule
                {
                    Id = "photos",
                    Prefix = "photos/",
                    Transitions = [new Transition { StorageClass = StorageClass.INFREQUENT_ACCESS, TransitionAfter = Duration.Days(90) }],
                },
                new LifecycleRule { Id = "abort-incomplete-uploads", AbortIncompleteMultipartUploadAfter = Duration.Days(2) },
            ],
        });

        var user = new User(this, "HostUser", new UserProps { UserName = "homeradar-host" });
        user.AddToPolicy(new PolicyStatement(new PolicyStatementProps
        {
            Sid = "BucketObjects",
            Actions = ["s3:PutObject", "s3:GetObject", "s3:DeleteObject", "s3:AbortMultipartUpload"],
            Resources = [bucket.ArnForObjects("*")],
        }));
        user.AddToPolicy(new PolicyStatement(new PolicyStatementProps
        {
            Sid = "BucketList",
            Actions = ["s3:ListBucket", "s3:GetBucketLocation"],
            Resources = [bucket.BucketArn],
        }));
        user.AddToPolicy(new PolicyStatement(new PolicyStatementProps
        {
            Sid = "Email",
            Actions = ["ses:SendEmail", "ses:SendRawEmail"],
            Resources = ["*"],
        }));

        if (props.BudgetEmail.Length > 0)
        {
            _ = new CfnBudget(this, "MonthlyBudget", new CfnBudgetProps
            {
                Budget = new CfnBudget.BudgetDataProperty
                {
                    BudgetName = "homeradar-monthly",
                    BudgetType = "COST",
                    TimeUnit = "MONTHLY",
                    BudgetLimit = new CfnBudget.SpendProperty { Amount = props.MonthlyBudgetUsd, Unit = "USD" },
                },
                NotificationsWithSubscribers =
                [
                    Alert(60, props.BudgetEmail),
                    Alert(100, props.BudgetEmail),
                ],
            });
        }

        _ = new CfnOutput(this, "BucketName", new CfnOutputProps { Value = bucket.BucketName });
        _ = new CfnOutput(this, "HostUserName", new CfnOutputProps { Value = user.UserName, Description = "Create an access key for this user in the console and put it in the host .env" });
    }

    private static CfnBudget.NotificationWithSubscribersProperty Alert(double thresholdPercent, string email) => new()
    {
        Notification = new CfnBudget.NotificationProperty
        {
            ComparisonOperator = "GREATER_THAN",
            NotificationType = "ACTUAL",
            Threshold = thresholdPercent,
            ThresholdType = "PERCENTAGE",
        },
        Subscribers = [new CfnBudget.SubscriberProperty { SubscriptionType = "EMAIL", Address = email }],
    };
}
