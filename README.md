# Jarvis

Jarvis is a backend Windows service that is ultimately a framework for communication.  By adding behaviors, new functionality can be added in the future.  Therefore Jarvis will be able to learn new responses over time.  Jarvis is made to be durable, thus why it is a Windows service.  Even when the computer is asleep Jarvis can still run and if the computer is rebooted Jarvis will restart along with it.  Jarvis has safeguards to handle errors in behaviors such that if you create a behavior for Jarvis and it throws an error, it will not terminate.

![example event parameter](https://github.com/github/docs/actions/workflows/main.yml/badge.svg?event=push)

For more info on Jarvis go to:
https://trello.com/b/d5shiZ23/jarvis

Jarvis uses ML.Net to construct most of its neural networks:
https://dotnet.microsoft.com/en-us/apps/machinelearning-ai/ml-dotnet
