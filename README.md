# AKKA non-blocking message processing.

ReceiveAsync creates wrapper task:
``` csharp
    public abstract class ReceiveActor : UntypedActor, IInitializableActor
    {
        // ...
        protected void ReceiveAsync<T>(Predicate<T> shouldHandle, Func<T, Task> handler)
        {
            Receive(WrapAsyncHandler(handler), shouldHandle);
        }

        // ...
        private Action<T> WrapAsyncHandler<T>(Func<T, Task> asyncHandler)
        {
            return m =>
            {
                Task Wrap() => asyncHandler(m);
                ActorTaskScheduler.RunTask(Wrap);
            };
        }
        // ...
    }
```

Then mailbox dispatcher is suspended:
``` csharp
    public class ActorTaskScheduler : TaskScheduler
    {
        // ...
        public static void RunTask(Func<Task> asyncAction)
        {
            var context = ActorCell.Current;

            if (context == null)
                throw new InvalidOperationException("RunTask must be called from an actor context.");

            var dispatcher = context.Dispatcher;

            //suspend the mailbox
            dispatcher.Suspend(context); 

            ActorTaskScheduler actorScheduler = context.TaskScheduler;
            actorScheduler.CurrentMessage = context.CurrentMessage;

            Task<Task>.Factory.StartNew(asyncAction, CancellationToken.None, TaskCreationOptions.None, actorScheduler)
                              .Unwrap()
                              .ContinueWith(parent =>
                              {
                                  Exception exception = GetTaskException(parent);

                                  if (exception == null)
                                  {
                                      dispatcher.Resume(context);

                                      context.CheckReceiveTimeout();
                                  }
                                  else
                                  {
                                      context.Self.AsInstanceOf<IInternalActorRef>().SendSystemMessage(new ActorTaskSchedulerMessage(exception, actorScheduler.CurrentMessage));
                                  }
                                  //clear the current message field of the scheduler
                                  actorScheduler.CurrentMessage = null;
                              }, actorScheduler);
        }
        // ...
```

Then PipeTo calls Sender.Tell with response message.
``` csharp
public static class PipeToSupport
    {
        public static Task PipeTo<T>(this Task<T> taskToPipe, ICanTell recipient, IActorRef sender = null, Func<T, object> success = null, Func<Exception, object> failure = null)
        {
            sender = sender ?? ActorRefs.NoSender;
            return taskToPipe.ContinueWith(tresult =>
            {
                if (tresult.IsCanceled || tresult.IsFaulted)
                    recipient.Tell(failure != null
                        ? failure(tresult.Exception)
                        : new Status.Failure(tresult.Exception), sender);
                else if (tresult.IsCompleted)
                    recipient.Tell(success != null
                        ? success(tresult.Result)
                        : tresult.Result, sender);
            }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }
    }
```

Some links:

- How to Do Asynchronous I/O with Akka.NET Actors Using PipeTo:
https://petabridge.com/blog/akkadotnet-async-actors-using-pipeto/
https://github.com/petabridge/akkadotnet-code-samples/blob/master/PipeTo/src/PipeTo.App/Actors/FeedValidatorActor.cs

- ASYNCHRONOUS AND CONCURRENT PROCESSING IN AKKA .NET ACTORS:
http://gigi.nullneuron.net/gigilabs/asynchronous-and-concurrent-processing-in-akka-net-actors/  

- Don't ask tell:
https://bartoszsypytkowski.com/dont-ask-tell-2/
