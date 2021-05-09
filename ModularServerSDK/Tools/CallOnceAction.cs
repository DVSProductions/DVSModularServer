using System;

namespace ModularServerSDK.Tools {
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
		private CallOnceAction me;
		/// <summary>
		/// Internal Action
		/// </summary>
		private readonly Action a;
		/// <summary>
		/// Creates a new CallOnceAction 
		/// </summary>
		/// <param name="call">Action that should only be called once</param>
		public CallOnceAction(Action call) {
			me = this;
			a = call;
		}
		/// <summary>
		/// Call the Action. Will only work once. Calling this twice will result in a exception
		/// </summary>
		public void Call() {
			lock (me) {
				me = null;
				a();
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
		private CallOnceAction<T> me;
		/// <summary>
		/// Internal Action
		/// </summary>
		private readonly Action<T> a;
		/// <summary>
		/// Creates a new CallOnceAction 
		/// </summary>
		/// <param name="call">Action that should only be called once</param>
		public CallOnceAction(Action<T> call) {
			me = this;
			a = call;
		}
		/// <summary>
		/// Call the Action. Will only work once. Calling this twice will result in a exception
		/// </summary>
		public void Call(T param) {
			lock (me) {
				a(param);
				me = null;
			}
		}
		/// <summary>
		/// Returns a new Call once Action that can be reset using the returned Action
		/// </summary>
		public static Tuple<CallOnceAction<T>, Action> CreateWithReset(Action<T> call) {
			var r = new CallOnceAction<T>(call);
			return new Tuple<CallOnceAction<T>, Action>(r, () => r.me = r);
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
		private CallOnceAction<T1, T2> me;
		/// <summary>
		/// Internal Action
		/// </summary>
		private readonly Action<T1, T2> a;
		/// <summary>
		/// Creates a new CallOnceAction with two parameters
		/// </summary>
		/// <param name="call">Action that should only be called once</param>
		public CallOnceAction(Action<T1, T2> call) {
			me = this;
			a = call;
		}
		/// <summary>
		/// Call the Action. Will only work once. Calling this twice will result in a exception
		/// </summary>
		public void Call(T1 p1, T2 p2) {
			lock (me) {
				me = null;
				a(p1, p2);
			}
		}
		/// <summary>
		/// Returns a new Call once Action that can be reset using the returned Action
		/// </summary>
		public static Tuple<CallOnceAction<T1, T2>, Action> CreateWithReset(Action<T1, T2> call) {
			var r = new CallOnceAction<T1, T2>(call);
			return new Tuple<CallOnceAction<T1, T2>, Action>(r, () => r.me = r);
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
		private CallOnceFunction<TRresult> me;
		/// <summary>
		/// Internal Function
		/// </summary>
		private readonly Func<TRresult> a;
		/// <summary>
		/// Creates a new CallOnceFunction
		/// </summary>
		/// <param name="call">Function that should only be called once</param>
		public CallOnceFunction(Func<TRresult> call) {
			me = this;
			a = call;
		}
		/// <summary>
		/// Call the Function. Will only work once. Calling this twice will result in a exception
		/// </summary>
		public TRresult Call() {
			lock (me) {
				me = null;
				return a();
			}
		}
		/// <summary>
		/// Returns a new Call once Function that can be reset using the returned Action
		/// </summary>
		public static Tuple<CallOnceFunction<TRresult>, Action> CreateWithReset(Func<TRresult> call) {
			var r = new CallOnceFunction<TRresult>(call);
			return new Tuple<CallOnceFunction<TRresult>, Action>(r, () => r.me = r);
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
		private CallOnceFunction<T, TResult> me;
		/// <summary>
		/// Internal Function
		/// </summary>
		private readonly Func<T, TResult> a;
		/// <summary>
		/// Creates a new CallOnceFunction with a parameter and a return value
		/// </summary>
		/// <param name="call">Function that should only be called once</param>
		public CallOnceFunction(Func<T, TResult> call) {
			me = this;
			a = call;
		}
		/// <summary>
		/// Call the Function. Will only work once. Calling this twice will result in a exception
		/// </summary>
		public TResult Call(T param) {
			lock (me) {
				me = null;
				return a(param);
			}
		}
		/// <summary>
		/// Returns a new Call once Function that can be reset using the returned Action
		/// </summary>
		public static Tuple<CallOnceFunction<T, TResult>, Action> CreateWithReset(Func<T, TResult> call) {
			var r = new CallOnceFunction<T, TResult>(call);
			return new Tuple<CallOnceFunction<T, TResult>, Action>(r, () => r.me = r);
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
		private CallOnceFunction<T1, T2, TResult> me;
		/// <summary>
		/// Internal Function
		/// </summary>
		private readonly Func<T1, T2, TResult> a;
		/// <summary>
		/// Creates a new CallOnceFunction with two parameters and a return value
		/// </summary>
		/// <param name="call">Function that should only be called once</param>
		public CallOnceFunction(Func<T1, T2, TResult> call) {
			me = this;
			a = call;
		}
		/// <summary>
		/// Call the Function. Will only work once. Calling this twice will result in a exception
		/// </summary>
		public TResult Call(T1 param1, T2 param2) {
			lock (me) {
				me = null;
				return a(param1, param2);
			}
		}
		/// <summary>
		/// Returns a new Call once Function that can be reset using the returned Action
		/// </summary>
		public static Tuple<CallOnceFunction<T1, T2, TResult>, Action> CreateWithReset(Func<T1, T2, TResult> call) {
			var r = new CallOnceFunction<T1, T2, TResult>(call);
			return new Tuple<CallOnceFunction<T1, T2, TResult>, Action>(r, () => r.me = r);
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
		TStorage store = null;
		/// <summary>
		/// Create a instance of a <see cref="SetOnceObject{TStorage}"/>
		/// </summary>
		public SetOnceObject() {
		}
		/// <summary>
		/// Set the read only value
		/// </summary>
		/// <param name="newValue">permanent value</param>
		public void Set(TStorage newValue) {
			if (store != null)
				throw new InvalidOperationException();
			store = newValue;
		}
		/// <summary>
		/// Set the read only value
		/// And receive a Action to change it later if necessary
		/// </summary>
		/// <param name="newValue">New value</param>
		public Action<TStorage> SetAndChangeLater(TStorage newValue) {
			if (store != null)
				throw new InvalidOperationException();
			store = newValue;
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
		/// <summary>
		/// Create a new <see cref="SetOnceStruct{TStorage}"/> where the caller gets a action that allows
		/// </summary>
		public static Tuple<SetOnceObject<TStorage>, Action<TStorage>> Create() {
			var r = new SetOnceObject<TStorage>();
			return new Tuple<SetOnceObject<TStorage>, Action<TStorage>>(r, (o) => r.store = o);
		}
		/// <summary>
		/// Allows implicit conversations to the contained value
		/// </summary>
		public static implicit operator TStorage(SetOnceObject<TStorage> targetStore) => targetStore == null ? default : targetStore.store;
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
		private SetOnceStruct<TStorage> me;
		/// <summary>
		/// Safe storage for the actual value
		/// </summary>
		TStorage store;
		/// <summary>
		/// Create a instance of a <see cref="SetOnceStruct{TStorage}"/>
		/// </summary>
		public SetOnceStruct() => me = this;
		/// <summary>
		/// Set the read only value
		/// </summary>
		/// <param name="newValue"></param>
		public void Set(TStorage newValue) {
			lock (me) {
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
			lock (me) {
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
		/// <summary>
		/// Create a new <see cref="SetOnceStruct{TStorage}"/> where the caller gets a action that allows
		/// </summary>
		public static Tuple<SetOnceStruct<TStorage>, Action<TStorage>> Create() {
			var r = new SetOnceStruct<TStorage>();
			return new Tuple<SetOnceStruct<TStorage>, Action<TStorage>>(r, (o) => r.store = o);
		}
		public static implicit operator TStorage(SetOnceStruct<TStorage> targetStore) => targetStore == null ? default : targetStore.store;
	}
}
