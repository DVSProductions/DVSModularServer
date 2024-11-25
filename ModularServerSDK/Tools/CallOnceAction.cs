namespace ModularServerSDK.Tools;
/// <summary>
/// A class that ensures that the contained <see cref="Action"/> can only be run once. 
/// Useful for setup Actions that shouldn't run twice.
/// don't lock this class! Locking it might result in a deadlock
/// </summary>
public class CallOnceAction {
	/// <summary>
	/// self. used to lock itself.
	/// If this is null <see cref="Call"/> will fail.
	/// </summary>
	private CallOnceAction? me;
	/// <summary>
	/// Internal Action
	/// </summary>
	private readonly Action action;
	/// <summary>
	/// Creates a new CallOnceAction 
	/// </summary>
	/// <param name="call">Action that should only be called once</param>
	public CallOnceAction(Action call) {
		me = this;
		action = call;
	}
	/// <summary>
	/// Call the Action. Will only work once. Calling this twice will result in a exception
	/// </summary>
	public void Call() {
		if(me == null)
			throw new InvalidOperationException("Action already called");
		lock(me) {
			me = null;
			action();
		}
	}
	/// <summary>
	/// Returns a new Call once Action that can be reset using the returned Action
	/// </summary>
	public static Tuple<CallOnceAction, Action> CreateWithReset(Action call) {
		var r = new CallOnceAction(call);
		return new Tuple<CallOnceAction, Action>(r, () => r.me = r);
	}
}
/// <summary>
/// A Class that ensures that the contained <see cref="Action{T}"/> can only be run once. 
/// Useful for setup Actions that shouldn't run twice.
/// This version Supports Actions with one parameter
/// don't lock this class! Locking it might result in a deadlock
/// </summary>
public class CallOnceAction<T> {
	/// <summary>
	/// self. used to lock itself.
	/// If this is null <see cref="Call"/> will fail.
	/// </summary>
	private CallOnceAction<T>? me;
	/// <summary>
	/// Internal Action
	/// </summary>
	private readonly Action<T> action;
	/// <summary>
	/// Creates a new CallOnceAction 
	/// </summary>
	/// <param name="call">Action that should only be called once</param>
	public CallOnceAction(Action<T> call) {
		me = this;
		action = call;
	}
	/// <summary>
	/// Returns a new Call once Action that can be reset using the returned Action
	/// </summary>
	public CallOnceAction(Action<T> call, out Action reset) {
		me = this;
		action = call;
		reset = () => me = this;
	}
	/// <summary>
	/// Call the Action. Will only work once. Calling this twice will result in a exception
	/// </summary>
	public void Call(T param) {
		if(me == null)
			throw new InvalidOperationException("Action already called");
		lock(me) {
			action(param);
			me = null;
		}
	}
}
/// <summary>
/// A Class that ensures that the contained <see cref="Action{T1, T2}"/> can only be run once. 
/// Useful for setup Actions that shouldn't run twice.
/// This version Supports Actions with two parameters
/// don't lock this class! Locking it might result in a deadlock
/// </summary>
public class CallOnceAction<T1, T2> {
	/// <summary>
	/// used to lock itself.
	/// If this is null <see cref="Call"/> will fail.
	/// </summary>
	private CallOnceAction<T1, T2>? me;
	/// <summary>
	/// Internal Action
	/// </summary>
	private readonly Action<T1, T2> action;
	/// <summary>
	/// Creates a new CallOnceAction with two parameters
	/// </summary>
	/// <param name="call">Action that should only be called once</param>
	public CallOnceAction(Action<T1, T2> call) {
		me = this;
		action = call;
	}
	/// <summary>
	/// Returns a new Call once Action that can be reset using the returned Action
	/// </summary>
	public CallOnceAction(Action<T1, T2> call, out Action reset) {
		me = this;
		action = call;
		reset = () => me = this;
	}
	/// <summary>
	/// Call the Action. Will only work once. Calling this twice will result in a exception
	/// </summary>
	public void Call(T1 p1, T2 p2) {
		if(me == null)
			throw new InvalidOperationException("Action already called");
		lock(me) {
			me = null;
			action(p1, p2);
		}
	}
}
/// <summary>
/// A Class that ensures that the contained <see cref="Func{TResult}"/> can only be run once. 
/// Useful for setup Function that shouldn't run twice.
/// don't lock this class! Locking it might result in a deadlock
/// </summary>
public class CallOnceFunction<TRresult> {
	/// <summary>
	/// used to lock itself.
	/// If this is null <see cref="Call"/> will fail.
	/// </summary>
	private CallOnceFunction<TRresult>? me;
	/// <summary>
	/// Internal Function
	/// </summary>
	private readonly Func<TRresult> action;
	/// <summary>
	/// Creates a new CallOnceFunction
	/// </summary>
	/// <param name="call">Function that should only be called once</param>
	public CallOnceFunction(Func<TRresult> call) {
		me = this;
		action = call;
	}
	/// <summary>
	/// Returns a new Call once Function that can be reset using the returned Action
	/// </summary>
	public CallOnceFunction(Func<TRresult> call, out Action reset) {
		me = this;
		action = call;
		reset = () => me = this;
	}
	/// <summary>
	/// Call the Function. Will only work once. Calling this twice will result in a exception
	/// </summary>
	public TRresult Call() {
		if(me == null)
			throw new InvalidOperationException("Function already called");
		lock(me) {
			me = null;
			return action();
		}
	}
}
/// <summary>
/// A Class that ensures that the contained <see cref="Func{T, TResult}"/> can only be run once. 
/// Useful for setup Function that shouldn't run twice.
/// don't lock this class! Locking it might result in a deadlock
/// </summary>
public class CallOnceFunction<T, TResult> {
	/// <summary>
	/// used to lock itself.
	/// If this is null <see cref="Call"/> will fail.
	/// </summary>
	private CallOnceFunction<T, TResult>? me;
	/// <summary>
	/// Internal Function
	/// </summary>
	private readonly Func<T, TResult> action;
	/// <summary>
	/// Creates a new CallOnceFunction with a parameter and a return value
	/// </summary>
	/// <param name="call">Function that should only be called once</param>
	public CallOnceFunction(Func<T, TResult> call) {
		me = this;
		action = call;
	}
	/// <summary>
	/// Returns a new Call once Function that can be reset using the returned Action
	/// </summary>
	public CallOnceFunction(Func<T, TResult> call, out Action reset) {
		me = this;
		action = call;
		reset = () => me = this;
	}
	/// <summary>
	/// Call the Function. Will only work once. Calling this twice will result in a exception
	/// </summary>
	public TResult Call(T param) {
		if(me == null)
			throw new InvalidOperationException("Function already called");
		lock(me) {
			me = null;
			return action(param);
		}
	}
}
/// <summary>
/// A Class that ensures that the contained <see cref="Func{T1, T2, TResult}"/> can only be run once. 
/// Useful for setup Function that shouldn't run twice.
/// don't lock this class! Locking it might result in a deadlock
/// </summary>
public class CallOnceFunction<T1, T2, TResult> {
	/// <summary>
	/// used to lock itself.
	/// If this is null <see cref="Call"/> will fail.
	/// </summary>
	private CallOnceFunction<T1, T2, TResult>? me;
	/// <summary>
	/// Internal Function
	/// </summary>
	private readonly Func<T1, T2, TResult> action;
	/// <summary>
	/// Creates a new CallOnceFunction with two parameters and a return value
	/// </summary>
	/// <param name="call">Function that should only be called once</param>
	public CallOnceFunction(Func<T1, T2, TResult> call) {
		me = this;
		action = call;
	}
	/// <summary>
	/// Returns a new Call once Function that can be reset using the returned Action
	/// </summary>
	public CallOnceFunction(Func<T1, T2, TResult> call, out Action reset) {
		me = this;
		action = call;
		reset = () => me = this;
	}
	/// <summary>
	/// Call the Function. Will only work once. Calling this twice will result in a exception
	/// </summary>
	public TResult Call(T1 param1, T2 param2) {
		if(me == null)
			throw new InvalidOperationException("Function already called");
		lock(me) {
			me = null;
			return action(param1, param2);
		}
	}
}
/// <summary>
/// Creates a Safe Object that can only be retrieved once set.
/// This allows for a caller to set a value permanently.
/// Useful for Static classes that should contain references to other stuff which never changes during 
/// the runtime of the application and should never be changed again, while exposing the Variable.
/// </summary>
/// <typeparam name="TStorage">Type of the stored value</typeparam>
public class SetOnceObject<TStorage> where TStorage : class {
	/// <summary>
	/// Safe storage for the actual value
	/// </summary>
	private TStorage? store;
	/// <summary>
	/// Create a instance of a <see cref="SetOnceObject{TStorage}"/>
	/// </summary>
	public SetOnceObject() {
	}
	/// <summary>
	/// Special constructor that allows the caller to set the value any time
	/// </summary>
	public SetOnceObject(out Action<TStorage> setValue) => setValue = (o) => store = o;
	/// <summary>
	/// Set the read only value
	/// </summary>
	/// <param name="newValue">permanent value</param>
	public void Set(TStorage newValue) {
		if(store != null)
			throw new InvalidOperationException("Object has already been set");
		store = newValue;
	}
	/// <summary>
	/// Set the read only value
	/// And receive a Action to change it later if necessary
	/// </summary>
	/// <param name="newValue">New value</param>
	public Action<TStorage> SetAndChangeLater(TStorage newValue) {
		if(store != null)
			throw new InvalidOperationException("Object has already been set");
		store = newValue;
		return (o) => store = o;
	}
	/// <summary>
	/// Get the stored value
	/// </summary>
	public TStorage Get() => store ?? throw new InvalidOperationException("Object has not been set yet");

	/// <summary>
	/// Get the stored value
	/// </summary>
	public TStorage ToTStorage() => Get();

	/// <summary>
	/// Allows implicit conversations to the contained value
	/// </summary>
	public static implicit operator TStorage(SetOnceObject<TStorage> targetStore) => targetStore.Get();
}
/// <summary>
/// Creates a Safe Object that can only be retrieved once set.
/// This allows for a caller to set a value permanently.
/// Useful for Static classes that should contain references to other stuff which never changes during 
/// the runtime of the application and should never be changed again, while exposing the Variable.
/// </summary>
/// <typeparam name="TStorage">Type of the stored value</typeparam>
public class SetOnceStruct<TStorage> where TStorage : struct {
	/// <summary>
	/// self. used to lock itself.
	/// If this is null <see cref="Call"/> will fail.
	/// </summary>
	private SetOnceStruct<TStorage>? me;

	/// <summary>
	/// Safe storage for the actual value
	/// </summary>
	private TStorage store;
	/// <summary>
	/// Create a instance of a <see cref="SetOnceStruct{TStorage}"/>
	/// </summary>
	public SetOnceStruct() => me = this;
	/// <summary>
	/// Special constructor that allows the caller to set the value any time
	/// </summary>
	/// <param name="setValue"></param>
	public SetOnceStruct(out Action<TStorage> setValue) {
		me = this;
		setValue = (o) => store = o;
	}
	/// <summary>
	/// Set the read only value
	/// </summary>
	/// <param name="newValue"></param>
	public void Set(TStorage newValue) {
		if(me == null)
			throw new InvalidOperationException("Object has already been set");
		lock(me) {
			me = null;
			store = newValue;
		}
	}
	/// <summary>
	/// Set the read only value
	/// And receive a Action to change it later if necessary
	/// </summary>
	/// <param name="newValue">New value</param>
	public Action<TStorage> SetAndChangeLater(TStorage newValue) {
		if(me == null)
			throw new InvalidOperationException("Object has already been set");
		lock(me) {
			me = null;
			store = newValue;
		}
		return (o) => store = o;
	}
	/// <summary>
	/// Get the stored value
	/// </summary>
	public TStorage Get() => store;
	/// <summary>
	/// Get the stored value
	/// </summary>
	public TStorage ToTStorage() => store;
	public static implicit operator TStorage(SetOnceStruct<TStorage> targetStore) => targetStore == null ? default : targetStore.store;
}
