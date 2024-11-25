namespace ModularServerSDK.Tools;
/// <summary>
/// This datastructure ensures that clients will always get the most up to date version of data.
/// Also it ensures that complicated operations only get executed when needed and not everytime a user requests something.
/// To accomplish this we have a generator that supplies compiled data to the buffer.
/// If outside things change you can let this datastructure know that the data needs to be refreshed asap by calling <see cref="AtomicBuffer{TData}.Invalidate"/>.
/// Data refreshing will be handled on a <see cref="AtomicBuffer{TData}.Get"/> call.
/// If the data gets invalidated while the Get is executing, the data won't get stored as the valid buffer.
/// </summary>
/// <typeparam name="TData">Type of the data to store</typeparam>
public class AtomicBuffer<TData> {
	/// <summary>
	/// Latest valid data
	/// </summary>
	private TData Buffer;

	/// <summary>
	/// Is the current buffer valid
	/// </summary>
	private bool valid = true;

	/// <summary>
	/// Atomic validity check.
	/// Gets Incremented whenever data gets invalidated. 
	/// Used to check if the data changed during compilation
	/// </summary>
	private int counter = 0;
	private readonly Func<TData> Generator;
	/// <summary>
	/// Create a new <see cref="AtomicBuffer{TData}"/>
	/// </summary>
	/// <param name="DataRegenerator">A function that will create the data we want to buffer</param>
	public AtomicBuffer(Func<TData> DataCompiler) {
		Generator = DataCompiler ?? throw new ArgumentNullException(nameof(DataCompiler), "A generator needs to be specified");
		Buffer = Generator();
	}
	/// <summary>
	/// Marks the current data as Invalid. 
	/// Call this EVERYTIME data used by the Compiler changes.
	/// </summary>
	public void Invalidate() {
		counter++;
		valid = false;
	}
	/// <summary>
	/// Get the current Valid buffer
	/// <para>If the buffer is valid recompile the data</para>
	/// </summary>
	/// <returns></returns>
	public TData Get() {
		if(valid)
			return Buffer;
		var mycount = counter;
		var tmp = Generator();
		if(mycount == counter && !valid) {
			valid = true;
			Buffer = tmp;
		}
		return tmp;
	}
	/// <summary>
	/// Support for implicit calls to get via conversion to the data type
	/// </summary>
	/// <param name="d"></param>
	public static implicit operator TData(AtomicBuffer<TData> d) => d.Get();
}
