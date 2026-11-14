Every registered source with its polling intervals and health. `status` moves to `Degraded` when a run returns no listings after
earlier successful runs or more than 20 % incomplete records (nothing is withdrawn in that case), and to `Failing` after three
consecutive failures; a Telegram alert is sent on every transition.
