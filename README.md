# TfsBot
Integration for TFS work items

## TFS -> TfsBot -> Slack

### Goals:
TfsBot is a service built on .NET Core.  The service registers itself with your TFS server.

When events happen in TFS, TfsBot uses it's configurable rules engine to determine whether this is an event that should be posted to slack or not.

If an event is determined to be slack-worthy, TfsBot uses highly configurable formatters to construct a slack post, and push it to a slack channel of your choosing.
