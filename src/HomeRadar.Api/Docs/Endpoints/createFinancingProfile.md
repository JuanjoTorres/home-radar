Creates a financing profile. `key` must be unique. Send an `Idempotency-Key` header to make retries safe: the same key with the
same body replays the original response (`Idempotent-Replayed: true`); the same key with a different body is rejected with 409.

Money is accepted as `{amount, currency}` with `currency` fixed to `EUR`. The response carries a weak `ETag` to use in later
`PUT` calls.
