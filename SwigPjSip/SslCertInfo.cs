//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 4.0.1
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------


public class SslCertInfo : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal SslCertInfo(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(SslCertInfo obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~SslCertInfo() {
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
          pjsua2PINVOKE.delete_SslCertInfo(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public uint version {
    set {
      pjsua2PINVOKE.SslCertInfo_version_set(swigCPtr, value);
    } 
    get {
      uint ret = pjsua2PINVOKE.SslCertInfo_version_get(swigCPtr);
      return ret;
    } 
  }

  public SWIGTYPE_p_unsigned_char serialNo {
    set {
      pjsua2PINVOKE.SslCertInfo_serialNo_set(swigCPtr, SWIGTYPE_p_unsigned_char.getCPtr(value));
    } 
    get {
      global::System.IntPtr cPtr = pjsua2PINVOKE.SslCertInfo_serialNo_get(swigCPtr);
      SWIGTYPE_p_unsigned_char ret = (cPtr == global::System.IntPtr.Zero) ? null : new SWIGTYPE_p_unsigned_char(cPtr, false);
      return ret;
    } 
  }

  public string subjectCn {
    set {
      pjsua2PINVOKE.SslCertInfo_subjectCn_set(swigCPtr, value);
      if (pjsua2PINVOKE.SWIGPendingException.Pending) throw pjsua2PINVOKE.SWIGPendingException.Retrieve();
    } 
    get {
      string ret = pjsua2PINVOKE.SslCertInfo_subjectCn_get(swigCPtr);
      if (pjsua2PINVOKE.SWIGPendingException.Pending) throw pjsua2PINVOKE.SWIGPendingException.Retrieve();
      return ret;
    } 
  }

  public string subjectInfo {
    set {
      pjsua2PINVOKE.SslCertInfo_subjectInfo_set(swigCPtr, value);
      if (pjsua2PINVOKE.SWIGPendingException.Pending) throw pjsua2PINVOKE.SWIGPendingException.Retrieve();
    } 
    get {
      string ret = pjsua2PINVOKE.SslCertInfo_subjectInfo_get(swigCPtr);
      if (pjsua2PINVOKE.SWIGPendingException.Pending) throw pjsua2PINVOKE.SWIGPendingException.Retrieve();
      return ret;
    } 
  }

  public string issuerCn {
    set {
      pjsua2PINVOKE.SslCertInfo_issuerCn_set(swigCPtr, value);
      if (pjsua2PINVOKE.SWIGPendingException.Pending) throw pjsua2PINVOKE.SWIGPendingException.Retrieve();
    } 
    get {
      string ret = pjsua2PINVOKE.SslCertInfo_issuerCn_get(swigCPtr);
      if (pjsua2PINVOKE.SWIGPendingException.Pending) throw pjsua2PINVOKE.SWIGPendingException.Retrieve();
      return ret;
    } 
  }

  public string issuerInfo {
    set {
      pjsua2PINVOKE.SslCertInfo_issuerInfo_set(swigCPtr, value);
      if (pjsua2PINVOKE.SWIGPendingException.Pending) throw pjsua2PINVOKE.SWIGPendingException.Retrieve();
    } 
    get {
      string ret = pjsua2PINVOKE.SslCertInfo_issuerInfo_get(swigCPtr);
      if (pjsua2PINVOKE.SWIGPendingException.Pending) throw pjsua2PINVOKE.SWIGPendingException.Retrieve();
      return ret;
    } 
  }

  public TimeVal validityStart {
    set {
      pjsua2PINVOKE.SslCertInfo_validityStart_set(swigCPtr, TimeVal.getCPtr(value));
    } 
    get {
      global::System.IntPtr cPtr = pjsua2PINVOKE.SslCertInfo_validityStart_get(swigCPtr);
      TimeVal ret = (cPtr == global::System.IntPtr.Zero) ? null : new TimeVal(cPtr, false);
      return ret;
    } 
  }

  public TimeVal validityEnd {
    set {
      pjsua2PINVOKE.SslCertInfo_validityEnd_set(swigCPtr, TimeVal.getCPtr(value));
    } 
    get {
      global::System.IntPtr cPtr = pjsua2PINVOKE.SslCertInfo_validityEnd_get(swigCPtr);
      TimeVal ret = (cPtr == global::System.IntPtr.Zero) ? null : new TimeVal(cPtr, false);
      return ret;
    } 
  }

  public bool validityGmt {
    set {
      pjsua2PINVOKE.SslCertInfo_validityGmt_set(swigCPtr, value);
    } 
    get {
      bool ret = pjsua2PINVOKE.SslCertInfo_validityGmt_get(swigCPtr);
      return ret;
    } 
  }

  public SslCertNameVector subjectAltName {
    set {
      pjsua2PINVOKE.SslCertInfo_subjectAltName_set(swigCPtr, SslCertNameVector.getCPtr(value));
    } 
    get {
      global::System.IntPtr cPtr = pjsua2PINVOKE.SslCertInfo_subjectAltName_get(swigCPtr);
      SslCertNameVector ret = (cPtr == global::System.IntPtr.Zero) ? null : new SslCertNameVector(cPtr, false);
      return ret;
    } 
  }

  public string raw {
    set {
      pjsua2PINVOKE.SslCertInfo_raw_set(swigCPtr, value);
      if (pjsua2PINVOKE.SWIGPendingException.Pending) throw pjsua2PINVOKE.SWIGPendingException.Retrieve();
    } 
    get {
      string ret = pjsua2PINVOKE.SslCertInfo_raw_get(swigCPtr);
      if (pjsua2PINVOKE.SWIGPendingException.Pending) throw pjsua2PINVOKE.SWIGPendingException.Retrieve();
      return ret;
    } 
  }

  public SslCertInfo() : this(pjsua2PINVOKE.new_SslCertInfo(), true) {
  }

  public bool isEmpty() {
    bool ret = pjsua2PINVOKE.SslCertInfo_isEmpty(swigCPtr);
    return ret;
  }

}
