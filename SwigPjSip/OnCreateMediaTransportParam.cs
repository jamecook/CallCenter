//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 4.0.1
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------


public class OnCreateMediaTransportParam : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal OnCreateMediaTransportParam(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(OnCreateMediaTransportParam obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~OnCreateMediaTransportParam() {
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
          pjsua2PINVOKE.delete_OnCreateMediaTransportParam(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public uint mediaIdx {
    set {
      pjsua2PINVOKE.OnCreateMediaTransportParam_mediaIdx_set(swigCPtr, value);
    } 
    get {
      uint ret = pjsua2PINVOKE.OnCreateMediaTransportParam_mediaIdx_get(swigCPtr);
      return ret;
    } 
  }

  public SWIGTYPE_p_void mediaTp {
    set {
      pjsua2PINVOKE.OnCreateMediaTransportParam_mediaTp_set(swigCPtr, SWIGTYPE_p_void.getCPtr(value));
    } 
    get {
      global::System.IntPtr cPtr = pjsua2PINVOKE.OnCreateMediaTransportParam_mediaTp_get(swigCPtr);
      SWIGTYPE_p_void ret = (cPtr == global::System.IntPtr.Zero) ? null : new SWIGTYPE_p_void(cPtr, false);
      return ret;
    } 
  }

  public uint flags {
    set {
      pjsua2PINVOKE.OnCreateMediaTransportParam_flags_set(swigCPtr, value);
    } 
    get {
      uint ret = pjsua2PINVOKE.OnCreateMediaTransportParam_flags_get(swigCPtr);
      return ret;
    } 
  }

  public OnCreateMediaTransportParam() : this(pjsua2PINVOKE.new_OnCreateMediaTransportParam(), true) {
  }

}
