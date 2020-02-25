//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 4.0.1
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------


public class AudioDevInfoVector2 : global::System.IDisposable, global::System.Collections.IEnumerable, global::System.Collections.Generic.IEnumerable<AudioDevInfo>
 {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal AudioDevInfoVector2(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(AudioDevInfoVector2 obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~AudioDevInfoVector2() {
    Dispose(false);
  }

  public void Dispose() {
    Dispose(true);
    global::System.GC.SuppressFinalize(this);
  }

  protected virtual void Dispose(bool disposing) {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          pjsua2PINVOKE.delete_AudioDevInfoVector2(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public AudioDevInfoVector2(global::System.Collections.IEnumerable c) : this() {
    if (c == null)
      throw new global::System.ArgumentNullException("c");
    foreach (AudioDevInfo element in c) {
      this.Add(element);
    }
  }

  public AudioDevInfoVector2(global::System.Collections.Generic.IEnumerable<AudioDevInfo> c) : this() {
    if (c == null)
      throw new global::System.ArgumentNullException("c");
    foreach (AudioDevInfo element in c) {
      this.Add(element);
    }
  }

  public bool IsFixedSize {
    get {
      return false;
    }
  }

  public bool IsReadOnly {
    get {
      return false;
    }
  }

  public AudioDevInfo this[int index]  {
    get {
      return getitem(index);
    }
    set {
      setitem(index, value);
    }
  }

  public int Capacity {
    get {
      return (int)capacity();
    }
    set {
      if (value < size())
        throw new global::System.ArgumentOutOfRangeException("Capacity");
      reserve((uint)value);
    }
  }

  public int Count {
    get {
      return (int)size();
    }
  }

  public bool IsSynchronized {
    get {
      return false;
    }
  }

  public void CopyTo(AudioDevInfo[] array)
  {
    CopyTo(0, array, 0, this.Count);
  }

  public void CopyTo(AudioDevInfo[] array, int arrayIndex)
  {
    CopyTo(0, array, arrayIndex, this.Count);
  }

  public void CopyTo(int index, AudioDevInfo[] array, int arrayIndex, int count)
  {
    if (array == null)
      throw new global::System.ArgumentNullException("array");
    if (index < 0)
      throw new global::System.ArgumentOutOfRangeException("index", "Value is less than zero");
    if (arrayIndex < 0)
      throw new global::System.ArgumentOutOfRangeException("arrayIndex", "Value is less than zero");
    if (count < 0)
      throw new global::System.ArgumentOutOfRangeException("count", "Value is less than zero");
    if (array.Rank > 1)
      throw new global::System.ArgumentException("Multi dimensional array.", "array");
    if (index+count > this.Count || arrayIndex+count > array.Length)
      throw new global::System.ArgumentException("Number of elements to copy is too large.");
    for (int i=0; i<count; i++)
      array.SetValue(getitemcopy(index+i), arrayIndex+i);
  }

  public AudioDevInfo[] ToArray() {
    AudioDevInfo[] array = new AudioDevInfo[this.Count];
    this.CopyTo(array);
    return array;
  }

  global::System.Collections.Generic.IEnumerator<AudioDevInfo> global::System.Collections.Generic.IEnumerable<AudioDevInfo>.GetEnumerator() {
    return new AudioDevInfoVector2Enumerator(this);
  }

  global::System.Collections.IEnumerator global::System.Collections.IEnumerable.GetEnumerator() {
    return new AudioDevInfoVector2Enumerator(this);
  }

  public AudioDevInfoVector2Enumerator GetEnumerator() {
    return new AudioDevInfoVector2Enumerator(this);
  }

  // Type-safe enumerator
  /// Note that the IEnumerator documentation requires an InvalidOperationException to be thrown
  /// whenever the collection is modified. This has been done for changes in the size of the
  /// collection but not when one of the elements of the collection is modified as it is a bit
  /// tricky to detect unmanaged code that modifies the collection under our feet.
  public sealed class AudioDevInfoVector2Enumerator : global::System.Collections.IEnumerator
    , global::System.Collections.Generic.IEnumerator<AudioDevInfo>
  {
    private AudioDevInfoVector2 collectionRef;
    private int currentIndex;
    private object currentObject;
    private int currentSize;

    public AudioDevInfoVector2Enumerator(AudioDevInfoVector2 collection) {
      collectionRef = collection;
      currentIndex = -1;
      currentObject = null;
      currentSize = collectionRef.Count;
    }

    // Type-safe iterator Current
    public AudioDevInfo Current {
      get {
        if (currentIndex == -1)
          throw new global::System.InvalidOperationException("Enumeration not started.");
        if (currentIndex > currentSize - 1)
          throw new global::System.InvalidOperationException("Enumeration finished.");
        if (currentObject == null)
          throw new global::System.InvalidOperationException("Collection modified.");
        return (AudioDevInfo)currentObject;
      }
    }

    // Type-unsafe IEnumerator.Current
    object global::System.Collections.IEnumerator.Current {
      get {
        return Current;
      }
    }

    public bool MoveNext() {
      int size = collectionRef.Count;
      bool moveOkay = (currentIndex+1 < size) && (size == currentSize);
      if (moveOkay) {
        currentIndex++;
        currentObject = collectionRef[currentIndex];
      } else {
        currentObject = null;
      }
      return moveOkay;
    }

    public void Reset() {
      currentIndex = -1;
      currentObject = null;
      if (collectionRef.Count != currentSize) {
        throw new global::System.InvalidOperationException("Collection modified.");
      }
    }

    public void Dispose() {
        currentIndex = -1;
        currentObject = null;
    }
  }

  public void Clear() {
    pjsua2PINVOKE.AudioDevInfoVector2_Clear(swigCPtr);
  }

  public void Add(AudioDevInfo x) {
    pjsua2PINVOKE.AudioDevInfoVector2_Add(swigCPtr, AudioDevInfo.getCPtr(x));
    if (pjsua2PINVOKE.SWIGPendingException.Pending) throw pjsua2PINVOKE.SWIGPendingException.Retrieve();
  }

  private uint size() {
    uint ret = pjsua2PINVOKE.AudioDevInfoVector2_size(swigCPtr);
    return ret;
  }

  private uint capacity() {
    uint ret = pjsua2PINVOKE.AudioDevInfoVector2_capacity(swigCPtr);
    return ret;
  }

  private void reserve(uint n) {
    pjsua2PINVOKE.AudioDevInfoVector2_reserve(swigCPtr, n);
  }

  public AudioDevInfoVector2() : this(pjsua2PINVOKE.new_AudioDevInfoVector2__SWIG_0(), true) {
  }

  public AudioDevInfoVector2(AudioDevInfoVector2 other) : this(pjsua2PINVOKE.new_AudioDevInfoVector2__SWIG_1(AudioDevInfoVector2.getCPtr(other)), true) {
    if (pjsua2PINVOKE.SWIGPendingException.Pending) throw pjsua2PINVOKE.SWIGPendingException.Retrieve();
  }

  public AudioDevInfoVector2(int capacity) : this(pjsua2PINVOKE.new_AudioDevInfoVector2__SWIG_2(capacity), true) {
    if (pjsua2PINVOKE.SWIGPendingException.Pending) throw pjsua2PINVOKE.SWIGPendingException.Retrieve();
  }

  private AudioDevInfo getitemcopy(int index) {
    AudioDevInfo ret = new AudioDevInfo(pjsua2PINVOKE.AudioDevInfoVector2_getitemcopy(swigCPtr, index), true);
    if (pjsua2PINVOKE.SWIGPendingException.Pending) throw pjsua2PINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  private AudioDevInfo getitem(int index) {
    AudioDevInfo ret = new AudioDevInfo(pjsua2PINVOKE.AudioDevInfoVector2_getitem(swigCPtr, index), false);
    if (pjsua2PINVOKE.SWIGPendingException.Pending) throw pjsua2PINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  private void setitem(int index, AudioDevInfo val) {
    pjsua2PINVOKE.AudioDevInfoVector2_setitem(swigCPtr, index, AudioDevInfo.getCPtr(val));
    if (pjsua2PINVOKE.SWIGPendingException.Pending) throw pjsua2PINVOKE.SWIGPendingException.Retrieve();
  }

  public void AddRange(AudioDevInfoVector2 values) {
    pjsua2PINVOKE.AudioDevInfoVector2_AddRange(swigCPtr, AudioDevInfoVector2.getCPtr(values));
    if (pjsua2PINVOKE.SWIGPendingException.Pending) throw pjsua2PINVOKE.SWIGPendingException.Retrieve();
  }

  public AudioDevInfoVector2 GetRange(int index, int count) {
    global::System.IntPtr cPtr = pjsua2PINVOKE.AudioDevInfoVector2_GetRange(swigCPtr, index, count);
    AudioDevInfoVector2 ret = (cPtr == global::System.IntPtr.Zero) ? null : new AudioDevInfoVector2(cPtr, true);
    if (pjsua2PINVOKE.SWIGPendingException.Pending) throw pjsua2PINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public void Insert(int index, AudioDevInfo x) {
    pjsua2PINVOKE.AudioDevInfoVector2_Insert(swigCPtr, index, AudioDevInfo.getCPtr(x));
    if (pjsua2PINVOKE.SWIGPendingException.Pending) throw pjsua2PINVOKE.SWIGPendingException.Retrieve();
  }

  public void InsertRange(int index, AudioDevInfoVector2 values) {
    pjsua2PINVOKE.AudioDevInfoVector2_InsertRange(swigCPtr, index, AudioDevInfoVector2.getCPtr(values));
    if (pjsua2PINVOKE.SWIGPendingException.Pending) throw pjsua2PINVOKE.SWIGPendingException.Retrieve();
  }

  public void RemoveAt(int index) {
    pjsua2PINVOKE.AudioDevInfoVector2_RemoveAt(swigCPtr, index);
    if (pjsua2PINVOKE.SWIGPendingException.Pending) throw pjsua2PINVOKE.SWIGPendingException.Retrieve();
  }

  public void RemoveRange(int index, int count) {
    pjsua2PINVOKE.AudioDevInfoVector2_RemoveRange(swigCPtr, index, count);
    if (pjsua2PINVOKE.SWIGPendingException.Pending) throw pjsua2PINVOKE.SWIGPendingException.Retrieve();
  }

  public static AudioDevInfoVector2 Repeat(AudioDevInfo value, int count) {
    global::System.IntPtr cPtr = pjsua2PINVOKE.AudioDevInfoVector2_Repeat(AudioDevInfo.getCPtr(value), count);
    AudioDevInfoVector2 ret = (cPtr == global::System.IntPtr.Zero) ? null : new AudioDevInfoVector2(cPtr, true);
    if (pjsua2PINVOKE.SWIGPendingException.Pending) throw pjsua2PINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public void Reverse() {
    pjsua2PINVOKE.AudioDevInfoVector2_Reverse__SWIG_0(swigCPtr);
  }

  public void Reverse(int index, int count) {
    pjsua2PINVOKE.AudioDevInfoVector2_Reverse__SWIG_1(swigCPtr, index, count);
    if (pjsua2PINVOKE.SWIGPendingException.Pending) throw pjsua2PINVOKE.SWIGPendingException.Retrieve();
  }

  public void SetRange(int index, AudioDevInfoVector2 values) {
    pjsua2PINVOKE.AudioDevInfoVector2_SetRange(swigCPtr, index, AudioDevInfoVector2.getCPtr(values));
    if (pjsua2PINVOKE.SWIGPendingException.Pending) throw pjsua2PINVOKE.SWIGPendingException.Retrieve();
  }

}
