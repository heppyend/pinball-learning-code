using System.Collections.Generic;
public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ AOT assemblies
	public static readonly IReadOnlyList<string> PatchedAOTAssemblyList = new List<string>
	{
		"DOTween.dll",
		"LC.Newtonsoft.Json.dll",
		"System.Core.dll",
		"System.dll",
		"UnityEngine.AssetBundleModule.dll",
		"UnityEngine.CoreModule.dll",
		"YooAsset.dll",
		"mscorlib.dll",
		"zxing.unity.dll",
	};
	// }}

	// {{ constraint implement type
	// }} 

	// {{ AOT generic types
	// DG.Tweening.Core.DOGetter<UnityEngine.Color>
	// DG.Tweening.Core.DOSetter<UnityEngine.Color>
	// System.Action<BNRoom.Static.TargetFinder.TargetInfo>
	// System.Action<System.Collections.Generic.KeyValuePair<int,ushort>>
	// System.Action<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Action<System.ValueTuple<int,float>>
	// System.Action<UnityEngine.EventSystems.RaycastResult>
	// System.Action<UnityEngine.Vector3>
	// System.Action<float>
	// System.Action<int,int,int,int>
	// System.Action<int,int,int>
	// System.Action<int,long>
	// System.Action<int,object,int>
	// System.Action<int,object>
	// System.Action<int>
	// System.Action<object,object,object,object>
	// System.Action<object,object>
	// System.Action<object>
	// System.Collections.Generic.ArraySortHelper<BNRoom.Static.TargetFinder.TargetInfo>
	// System.Collections.Generic.ArraySortHelper<System.Collections.Generic.KeyValuePair<int,ushort>>
	// System.Collections.Generic.ArraySortHelper<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.ArraySortHelper<System.ValueTuple<int,float>>
	// System.Collections.Generic.ArraySortHelper<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.ArraySortHelper<UnityEngine.Vector3>
	// System.Collections.Generic.ArraySortHelper<int>
	// System.Collections.Generic.ArraySortHelper<object>
	// System.Collections.Generic.Comparer<BNRoom.Static.TargetFinder.TargetInfo>
	// System.Collections.Generic.Comparer<System.Collections.Generic.KeyValuePair<int,ushort>>
	// System.Collections.Generic.Comparer<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.Comparer<System.ValueTuple<int,float>>
	// System.Collections.Generic.Comparer<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.Comparer<UnityEngine.Vector3>
	// System.Collections.Generic.Comparer<float>
	// System.Collections.Generic.Comparer<int>
	// System.Collections.Generic.Comparer<object>
	// System.Collections.Generic.Dictionary.Enumerator<int,float>
	// System.Collections.Generic.Dictionary.Enumerator<int,int>
	// System.Collections.Generic.Dictionary.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,float>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,int>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.KeyCollection<int,float>
	// System.Collections.Generic.Dictionary.KeyCollection<int,int>
	// System.Collections.Generic.Dictionary.KeyCollection<int,object>
	// System.Collections.Generic.Dictionary.KeyCollection<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,float>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,int>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection<int,float>
	// System.Collections.Generic.Dictionary.ValueCollection<int,int>
	// System.Collections.Generic.Dictionary.ValueCollection<int,object>
	// System.Collections.Generic.Dictionary.ValueCollection<object,object>
	// System.Collections.Generic.Dictionary<int,float>
	// System.Collections.Generic.Dictionary<int,int>
	// System.Collections.Generic.Dictionary<int,object>
	// System.Collections.Generic.Dictionary<object,object>
	// System.Collections.Generic.EqualityComparer<float>
	// System.Collections.Generic.EqualityComparer<int>
	// System.Collections.Generic.EqualityComparer<object>
	// System.Collections.Generic.ICollection<BNRoom.Static.TargetFinder.TargetInfo>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,float>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,ushort>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.ICollection<System.ValueTuple<int,float>>
	// System.Collections.Generic.ICollection<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.ICollection<UnityEngine.Vector3>
	// System.Collections.Generic.ICollection<int>
	// System.Collections.Generic.ICollection<object>
	// System.Collections.Generic.IComparer<BNRoom.Static.TargetFinder.TargetInfo>
	// System.Collections.Generic.IComparer<System.Collections.Generic.KeyValuePair<int,ushort>>
	// System.Collections.Generic.IComparer<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IComparer<System.ValueTuple<int,float>>
	// System.Collections.Generic.IComparer<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.IComparer<UnityEngine.Vector3>
	// System.Collections.Generic.IComparer<float>
	// System.Collections.Generic.IComparer<int>
	// System.Collections.Generic.IComparer<object>
	// System.Collections.Generic.IDictionary<object,object>
	// System.Collections.Generic.IEnumerable<BNRoom.Static.TargetFinder.TargetInfo>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,float>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,ushort>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerable<System.ValueTuple<int,float>>
	// System.Collections.Generic.IEnumerable<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.IEnumerable<UnityEngine.Vector3>
	// System.Collections.Generic.IEnumerable<int>
	// System.Collections.Generic.IEnumerable<object>
	// System.Collections.Generic.IEnumerator<BNRoom.Static.TargetFinder.TargetInfo>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,float>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,ushort>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerator<System.ValueTuple<int,float>>
	// System.Collections.Generic.IEnumerator<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.IEnumerator<UnityEngine.Vector3>
	// System.Collections.Generic.IEnumerator<int>
	// System.Collections.Generic.IEnumerator<object>
	// System.Collections.Generic.IEqualityComparer<int>
	// System.Collections.Generic.IEqualityComparer<object>
	// System.Collections.Generic.IList<BNRoom.Static.TargetFinder.TargetInfo>
	// System.Collections.Generic.IList<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.IList<System.Collections.Generic.KeyValuePair<int,ushort>>
	// System.Collections.Generic.IList<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IList<System.ValueTuple<int,float>>
	// System.Collections.Generic.IList<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.IList<UnityEngine.Vector3>
	// System.Collections.Generic.IList<int>
	// System.Collections.Generic.IList<object>
	// System.Collections.Generic.KeyValuePair<int,float>
	// System.Collections.Generic.KeyValuePair<int,int>
	// System.Collections.Generic.KeyValuePair<int,object>
	// System.Collections.Generic.KeyValuePair<int,ushort>
	// System.Collections.Generic.KeyValuePair<object,object>
	// System.Collections.Generic.List.Enumerator<BNRoom.Static.TargetFinder.TargetInfo>
	// System.Collections.Generic.List.Enumerator<System.Collections.Generic.KeyValuePair<int,ushort>>
	// System.Collections.Generic.List.Enumerator<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.List.Enumerator<System.ValueTuple<int,float>>
	// System.Collections.Generic.List.Enumerator<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.List.Enumerator<UnityEngine.Vector3>
	// System.Collections.Generic.List.Enumerator<int>
	// System.Collections.Generic.List.Enumerator<object>
	// System.Collections.Generic.List<BNRoom.Static.TargetFinder.TargetInfo>
	// System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<int,ushort>>
	// System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.List<System.ValueTuple<int,float>>
	// System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.List<UnityEngine.Vector3>
	// System.Collections.Generic.List<int>
	// System.Collections.Generic.List<object>
	// System.Collections.Generic.ObjectComparer<BNRoom.Static.TargetFinder.TargetInfo>
	// System.Collections.Generic.ObjectComparer<System.Collections.Generic.KeyValuePair<int,ushort>>
	// System.Collections.Generic.ObjectComparer<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<int,float>>
	// System.Collections.Generic.ObjectComparer<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.ObjectComparer<UnityEngine.Vector3>
	// System.Collections.Generic.ObjectComparer<float>
	// System.Collections.Generic.ObjectComparer<int>
	// System.Collections.Generic.ObjectComparer<object>
	// System.Collections.Generic.ObjectEqualityComparer<float>
	// System.Collections.Generic.ObjectEqualityComparer<int>
	// System.Collections.Generic.ObjectEqualityComparer<object>
	// System.Collections.Generic.SortedDictionary.<>c__DisplayClass34_0<object,object>
	// System.Collections.Generic.SortedDictionary.<>c__DisplayClass34_1<object,object>
	// System.Collections.Generic.SortedDictionary.Enumerator<object,object>
	// System.Collections.Generic.SortedDictionary.KeyCollection.<>c__DisplayClass5_0<object,object>
	// System.Collections.Generic.SortedDictionary.KeyCollection.<>c__DisplayClass6_0<object,object>
	// System.Collections.Generic.SortedDictionary.KeyCollection.Enumerator<object,object>
	// System.Collections.Generic.SortedDictionary.KeyCollection<object,object>
	// System.Collections.Generic.SortedDictionary.KeyValuePairComparer<object,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection.<>c__DisplayClass5_0<object,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection.<>c__DisplayClass6_0<object,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection.Enumerator<object,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection<object,object>
	// System.Collections.Generic.SortedDictionary<object,object>
	// System.Collections.Generic.SortedSet.<>c__DisplayClass52_0<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.SortedSet.<>c__DisplayClass53_0<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.SortedSet.Enumerator<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.SortedSet.Node<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.SortedSet<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.Stack.Enumerator<UIManager.UIInfoData>
	// System.Collections.Generic.Stack.Enumerator<object>
	// System.Collections.Generic.Stack<UIManager.UIInfoData>
	// System.Collections.Generic.Stack<object>
	// System.Collections.Generic.TreeSet<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.TreeWalkPredicate<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.ObjectModel.ReadOnlyCollection<BNRoom.Static.TargetFinder.TargetInfo>
	// System.Collections.ObjectModel.ReadOnlyCollection<System.Collections.Generic.KeyValuePair<int,ushort>>
	// System.Collections.ObjectModel.ReadOnlyCollection<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.ObjectModel.ReadOnlyCollection<System.ValueTuple<int,float>>
	// System.Collections.ObjectModel.ReadOnlyCollection<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.ObjectModel.ReadOnlyCollection<UnityEngine.Vector3>
	// System.Collections.ObjectModel.ReadOnlyCollection<int>
	// System.Collections.ObjectModel.ReadOnlyCollection<object>
	// System.Comparison<BNRoom.Static.TargetFinder.TargetInfo>
	// System.Comparison<System.Collections.Generic.KeyValuePair<int,ushort>>
	// System.Comparison<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Comparison<System.ValueTuple<int,float>>
	// System.Comparison<UnityEngine.EventSystems.RaycastResult>
	// System.Comparison<UnityEngine.Vector3>
	// System.Comparison<int>
	// System.Comparison<object>
	// System.Func<BNRoom.Static.TargetFinder.TargetInfo,float>
	// System.Func<System.Threading.Tasks.VoidTaskResult>
	// System.Func<byte>
	// System.Func<object,System.Threading.Tasks.VoidTaskResult>
	// System.Func<object,byte>
	// System.Func<object,object,object>
	// System.Func<object,object>
	// System.Func<object>
	// System.Linq.Buffer<BNRoom.Static.TargetFinder.TargetInfo>
	// System.Linq.Enumerable.<CastIterator>d__99<object>
	// System.Linq.Enumerable.Iterator<object>
	// System.Linq.Enumerable.WhereArrayIterator<object>
	// System.Linq.Enumerable.WhereEnumerableIterator<object>
	// System.Linq.Enumerable.WhereListIterator<object>
	// System.Linq.Enumerable.WhereSelectArrayIterator<object,object>
	// System.Linq.Enumerable.WhereSelectEnumerableIterator<object,object>
	// System.Linq.Enumerable.WhereSelectListIterator<object,object>
	// System.Linq.EnumerableSorter<BNRoom.Static.TargetFinder.TargetInfo,float>
	// System.Linq.EnumerableSorter<BNRoom.Static.TargetFinder.TargetInfo>
	// System.Linq.OrderedEnumerable.<GetEnumerator>d__1<BNRoom.Static.TargetFinder.TargetInfo>
	// System.Linq.OrderedEnumerable<BNRoom.Static.TargetFinder.TargetInfo,float>
	// System.Linq.OrderedEnumerable<BNRoom.Static.TargetFinder.TargetInfo>
	// System.Nullable<System.DateTime>
	// System.Nullable<UnityEngine.Color>
	// System.Nullable<byte>
	// System.Nullable<double>
	// System.Nullable<int>
	// System.Nullable<long>
	// System.Predicate<BNRoom.Static.TargetFinder.TargetInfo>
	// System.Predicate<System.Collections.Generic.KeyValuePair<int,ushort>>
	// System.Predicate<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Predicate<System.ValueTuple<int,float>>
	// System.Predicate<UnityEngine.EventSystems.RaycastResult>
	// System.Predicate<UnityEngine.Vector3>
	// System.Predicate<int>
	// System.Predicate<object>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.Threading.Tasks.VoidTaskResult>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<object>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.Threading.Tasks.VoidTaskResult>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<object>
	// System.Runtime.CompilerServices.TaskAwaiter<System.Threading.Tasks.VoidTaskResult>
	// System.Runtime.CompilerServices.TaskAwaiter<object>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.Threading.Tasks.VoidTaskResult>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<object>
	// System.Threading.Tasks.Task<System.Threading.Tasks.VoidTaskResult>
	// System.Threading.Tasks.Task<object>
	// System.Threading.Tasks.TaskCompletionSource<object>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.Threading.Tasks.VoidTaskResult>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<object>
	// System.Threading.Tasks.TaskFactory<System.Threading.Tasks.VoidTaskResult>
	// System.Threading.Tasks.TaskFactory<object>
	// System.ValueTuple<int,float>
	// System.ValueTuple<int,int>
	// ZXing.BarcodeWriter<object>
	// ZXing.Rendering.IBarcodeRenderer<object>
	// }}

	public void RefMethods()
	{
		// object DG.Tweening.TweenExtensions.Pause<object>(object)
		// object DG.Tweening.TweenExtensions.Play<object>(object)
		// object DG.Tweening.TweenSettingsExtensions.From<object>(object)
		// object DG.Tweening.TweenSettingsExtensions.OnComplete<object>(object,DG.Tweening.TweenCallback)
		// object DG.Tweening.TweenSettingsExtensions.OnRewind<object>(object,DG.Tweening.TweenCallback)
		// object DG.Tweening.TweenSettingsExtensions.SetAutoKill<object>(object,bool)
		// object DG.Tweening.TweenSettingsExtensions.SetEase<object>(object,DG.Tweening.Ease)
		// object DG.Tweening.TweenSettingsExtensions.SetLoops<object>(object,int,DG.Tweening.LoopType)
		// object DG.Tweening.TweenSettingsExtensions.SetRelative<object>(object,bool)
		// object LC.Newtonsoft.Json.JsonConvert.DeserializeObject<object>(string)
		// object LC.Newtonsoft.Json.JsonConvert.DeserializeObject<object>(string,LC.Newtonsoft.Json.JsonSerializerSettings)
		// double LC.Newtonsoft.Json.Linq.JToken.ToObject<double>()
		// int LC.Newtonsoft.Json.Linq.JToken.ToObject<int>()
		// long LC.Newtonsoft.Json.Linq.JToken.ToObject<long>()
		// object LC.Newtonsoft.Json.Linq.JToken.ToObject<object>()
		// uint LC.Newtonsoft.Json.Linq.JToken.ToObject<uint>()
		// object System.Activator.CreateInstance<object>()
		// object[] System.Array.Empty<object>()
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Cast<object>(System.Collections.IEnumerable)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.CastIterator<object>(System.Collections.IEnumerable)
		// int System.Linq.Enumerable.Count<object>(System.Collections.Generic.IEnumerable<object>)
		// object System.Linq.Enumerable.First<object>(System.Collections.Generic.IEnumerable<object>)
		// System.Collections.Generic.KeyValuePair<int,object> System.Linq.Enumerable.Last<System.Collections.Generic.KeyValuePair<int,object>>(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,object>>)
		// object System.Linq.Enumerable.Last<object>(System.Collections.Generic.IEnumerable<object>)
		// System.Linq.IOrderedEnumerable<BNRoom.Static.TargetFinder.TargetInfo> System.Linq.Enumerable.OrderBy<BNRoom.Static.TargetFinder.TargetInfo,float>(System.Collections.Generic.IEnumerable<BNRoom.Static.TargetFinder.TargetInfo>,System.Func<BNRoom.Static.TargetFinder.TargetInfo,float>)
		// System.Linq.IOrderedEnumerable<BNRoom.Static.TargetFinder.TargetInfo> System.Linq.Enumerable.OrderByDescending<BNRoom.Static.TargetFinder.TargetInfo,float>(System.Collections.Generic.IEnumerable<BNRoom.Static.TargetFinder.TargetInfo>,System.Func<BNRoom.Static.TargetFinder.TargetInfo,float>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Select<object,object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,object>)
		// System.Collections.Generic.List<BNRoom.Static.TargetFinder.TargetInfo> System.Linq.Enumerable.ToList<BNRoom.Static.TargetFinder.TargetInfo>(System.Collections.Generic.IEnumerable<BNRoom.Static.TargetFinder.TargetInfo>)
		// System.Collections.Generic.List<int> System.Linq.Enumerable.ToList<int>(System.Collections.Generic.IEnumerable<int>)
		// System.Collections.Generic.List<object> System.Linq.Enumerable.ToList<object>(System.Collections.Generic.IEnumerable<object>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Where<object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,bool>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Iterator<object>.Select<object>(System.Func<object,object>)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,AssetUpdater.<DownloadAsync>d__23>(System.Runtime.CompilerServices.TaskAwaiter&,AssetUpdater.<DownloadAsync>d__23&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,AssetUpdater.<StartCheckFileListAsync>d__14>(System.Runtime.CompilerServices.TaskAwaiter&,AssetUpdater.<StartCheckFileListAsync>d__14&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,AssetUpdater.<TryLoadNextBundle>d__19>(System.Runtime.CompilerServices.TaskAwaiter&,AssetUpdater.<TryLoadNextBundle>d__19&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,BundleManager.<Init1>d__5>(System.Runtime.CompilerServices.TaskAwaiter&,BundleManager.<Init1>d__5&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,BundleManager.<LoadManifest1>d__8>(System.Runtime.CompilerServices.TaskAwaiter&,BundleManager.<LoadManifest1>d__8&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,UIManager.<LoadAndInstantiateUI>d__24>(System.Runtime.CompilerServices.TaskAwaiter<object>&,UIManager.<LoadAndInstantiateUI>d__24&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,UIManager.<LoadAndInstantiateUIAsync>d__22>(System.Runtime.CompilerServices.TaskAwaiter<object>&,UIManager.<LoadAndInstantiateUIAsync>d__22&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter,AssetUpdater.<DownloadAsync>d__23>(System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter&,AssetUpdater.<DownloadAsync>d__23&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,AssetUpdater.<DownloadAsync>d__23>(System.Runtime.CompilerServices.TaskAwaiter&,AssetUpdater.<DownloadAsync>d__23&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,AssetUpdater.<StartCheckFileListAsync>d__14>(System.Runtime.CompilerServices.TaskAwaiter&,AssetUpdater.<StartCheckFileListAsync>d__14&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,AssetUpdater.<TryLoadNextBundle>d__19>(System.Runtime.CompilerServices.TaskAwaiter&,AssetUpdater.<TryLoadNextBundle>d__19&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,BundleManager.<Init1>d__5>(System.Runtime.CompilerServices.TaskAwaiter&,BundleManager.<Init1>d__5&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,BundleManager.<LoadManifest1>d__8>(System.Runtime.CompilerServices.TaskAwaiter&,BundleManager.<LoadManifest1>d__8&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,UIManager.<LoadAndInstantiateUI>d__24>(System.Runtime.CompilerServices.TaskAwaiter<object>&,UIManager.<LoadAndInstantiateUI>d__24&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,UIManager.<LoadAndInstantiateUIAsync>d__22>(System.Runtime.CompilerServices.TaskAwaiter<object>&,UIManager.<LoadAndInstantiateUIAsync>d__22&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter,AssetUpdater.<DownloadAsync>d__23>(System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter&,AssetUpdater.<DownloadAsync>d__23&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<AssetUpdater.<DownloadAsync>d__23>(AssetUpdater.<DownloadAsync>d__23&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<AssetUpdater.<StartCheckFileListAsync>d__14>(AssetUpdater.<StartCheckFileListAsync>d__14&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<AssetUpdater.<TryLoadNextBundle>d__19>(AssetUpdater.<TryLoadNextBundle>d__19&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<BundleManager.<Init1>d__5>(BundleManager.<Init1>d__5&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<BundleManager.<LoadManifest1>d__8>(BundleManager.<LoadManifest1>d__8&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<UIManager.<LoadAndInstantiateUI>d__24>(UIManager.<LoadAndInstantiateUI>d__24&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<UIManager.<LoadAndInstantiateUIAsync>d__22>(UIManager.<LoadAndInstantiateUIAsync>d__22&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,AppRoot.<OnABUpdateComplete1>d__26>(System.Runtime.CompilerServices.TaskAwaiter&,AppRoot.<OnABUpdateComplete1>d__26&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,AssetUpdater.<OnBundleDone>d__20>(System.Runtime.CompilerServices.TaskAwaiter&,AssetUpdater.<OnBundleDone>d__20&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,AssetUpdater.<OnFileListDone>d__16>(System.Runtime.CompilerServices.TaskAwaiter&,AssetUpdater.<OnFileListDone>d__16&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,AssetUpdater.<StartHotUpdateDownload>d__13>(System.Runtime.CompilerServices.TaskAwaiter&,AssetUpdater.<StartHotUpdateDownload>d__13&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,AssetUpdater.<StartUpdate>d__12>(System.Runtime.CompilerServices.TaskAwaiter&,AssetUpdater.<StartUpdate>d__12&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,JBPROTO.NetComponentImpl.<checkConnectWithTimeout>d__6>(System.Runtime.CompilerServices.TaskAwaiter&,JBPROTO.NetComponentImpl.<checkConnectWithTimeout>d__6&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,TableLoadHelper.<_asyncLoadFromNet>d__12>(System.Runtime.CompilerServices.TaskAwaiter&,TableLoadHelper.<_asyncLoadFromNet>d__12&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,NetController.<asyncExecuteWebRequest>d__12<object>>(System.Runtime.CompilerServices.TaskAwaiter<object>&,NetController.<asyncExecuteWebRequest>d__12<object>&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,UIManager.<PreloadUI>d__9>(System.Runtime.CompilerServices.TaskAwaiter<object>&,UIManager.<PreloadUI>d__9&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<AppRoot.<OnABUpdateComplete1>d__26>(AppRoot.<OnABUpdateComplete1>d__26&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<AssetUpdater.<OnBundleDone>d__20>(AssetUpdater.<OnBundleDone>d__20&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<AssetUpdater.<OnFileListDone>d__16>(AssetUpdater.<OnFileListDone>d__16&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<AssetUpdater.<StartHotUpdateDownload>d__13>(AssetUpdater.<StartHotUpdateDownload>d__13&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<AssetUpdater.<StartUpdate>d__12>(AssetUpdater.<StartUpdate>d__12&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<JBPROTO.NetComponentImpl.<checkConnectWithTimeout>d__6>(JBPROTO.NetComponentImpl.<checkConnectWithTimeout>d__6&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<NetController.<asyncExecuteWebRequest>d__12<object>>(NetController.<asyncExecuteWebRequest>d__12<object>&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<TableLoadHelper.<_asyncLoadFromNet>d__12>(TableLoadHelper.<_asyncLoadFromNet>d__12&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<UIManager.<OpenUI1>d__20>(UIManager.<OpenUI1>d__20&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<UIManager.<OpenUI>d__18>(UIManager.<OpenUI>d__18&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<UIManager.<PreloadUI>d__9>(UIManager.<PreloadUI>d__9&)
		// object& System.Runtime.CompilerServices.Unsafe.As<object,object>(object&)
		// System.Void* System.Runtime.CompilerServices.Unsafe.AsPointer<object>(object&)
		// System.Threading.Tasks.Task<object> System.Threading.Tasks.Task.Run<object>(System.Func<object>)
		// object UnityEngine.AssetBundle.LoadAsset<object>(string)
		// UnityEngine.AssetBundleRequest UnityEngine.AssetBundle.LoadAssetAsync<object>(string)
		// object UnityEngine.Component.GetComponent<object>()
		// object UnityEngine.GameObject.AddComponent<object>()
		// object UnityEngine.GameObject.GetComponent<object>()
		// object UnityEngine.Object.FindObjectOfType<object>()
		// object UnityEngine.Object.Instantiate<object>(object)
		// object UnityEngine.Object.Instantiate<object>(object,UnityEngine.Transform)
		// object UnityEngine.Object.Instantiate<object>(object,UnityEngine.Transform,bool)
		// object UnityEngine.Object.Instantiate<object>(object,UnityEngine.Vector3,UnityEngine.Quaternion)
		// object UnityEngine.Resources.Load<object>(string)
		// UnityEngine.ResourceRequest UnityEngine.Resources.LoadAsync<object>(string)
		// YooAsset.AssetHandle YooAsset.ResourcePackage.LoadAssetAsync<object>(string,uint)
		// YooAsset.AssetHandle YooAsset.YooAssets.LoadAssetAsync<object>(string,uint)
	}
}