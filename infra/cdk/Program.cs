using Amazon.CDK;
using HomeRadar.Cdk;

var app = new App();
_ = new HomeRadarStack(app, "HomeRadar", new HomeRadarStackProps
{
    Env = new Amazon.CDK.Environment
    {
        Account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
        Region = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION") ?? "eu-west-1",
    },
    BudgetEmail = System.Environment.GetEnvironmentVariable("HOMERADAR_BUDGET_EMAIL") ?? string.Empty,
    MonthlyBudgetUsd = double.TryParse(System.Environment.GetEnvironmentVariable("HOMERADAR_BUDGET_USD"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var budget) ? budget : 15,
});
app.Synth();
