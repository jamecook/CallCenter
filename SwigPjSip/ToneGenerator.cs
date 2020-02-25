//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 4.0.1
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------


public class ToneGenerator : AudioMedia {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;

  internal ToneGenerator(global::System.IntPtr cPtr, bool cMemoryOwn) : base(pjsua2PINVOKE.ToneGenerator_SWIGUpcast(cPtr), cMemoryOwn) {
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(ToneGenerator obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  protected override void Dispose(bool disposing) {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          pjsua2PINVOKE.delete_ToneGenerator(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      base.Dispose(disposing);
    }
  }

  public ToneGenerator() : this(pjsua2PINVOKE.new_ToneGenerator(), true) {
  }

  public void createToneGenerator(uint clock_rate, uint channel_count) {
    pjsua2PINVOKE.ToneGenerator_createToneGenerator__SWIG_0(swigCPtr, clock_rate, channel_count);
    if (pjsua2PINVOKE.SWIGPendingException.Pending) throw pjsua2PINVOKE.SWIGPendingException.Retrieve();
  }

  public void createToneGenerator(uint clock_rate) {
    pjsua2PINVOKE.ToneGenerator_createToneGenerator__SWIG_1(swigCPtr, clock_rate);
    if (pjsua2PINVOKE.SWIGPendingException.Pending) throw pjsua2PINVOKE.SWIGPendingException.Retrieve();
  }

  public void createToneGenerator() {
    pjsua2PINVOKE.ToneGenerator_createToneGenerator__SWIG_2(swigCPtr);
    if (pjsua2PINVOKE.SWIGPendingException.Pending) throw pjsua2PINVOKE.SWIGPendingException.Retrieve();
  }

  public bool isBusy() {
    bool ret = pjsua2PINVOKE.ToneGenerator_isBusy(swigCPtr);
    return ret;
  }

  public void stop() {
    pjsua2PINVOKE.ToneGenerator_stop(swigCPtr);
    if (pjsua2PINVOKE.SWIGPendingException.Pending) throw pjsua2PINVOKE.SWIGPendingException.Retrieve();
  }

  public void rewind() {
    pjsua2PINVOKE.ToneGenerator_rewind(swigCPtr);
    if (pjsua2PINVOKE.SWIGPendingException.Pending) throw pjsua2PINVOKE.SWIGPendingException.Retrieve();
  }

  public void play(ToneDescVector tones, bool loop) {
    pjsua2PINVOKE.ToneGenerator_play__SWIG_0(swigCPtr, ToneDescVector.getCPtr(tones), loop);
    if (pjsua2PINVOKE.SWIGPendingException.Pending) throw pjsua2PINVOKE.SWIGPendingException.Retrieve();
  }

  public void play(ToneDescVector tones) {
    pjsua2PINVOKE.ToneGenerator_play__SWIG_1(swigCPtr, ToneDescVector.getCPtr(tones));
    if (pjsua2PINVOKE.SWIGPendingException.Pending) throw pjsua2PINVOKE.SWIGPendingException.Retrieve();
  }

  public void playDigits(ToneDigitVector digits, bool loop) {
    pjsua2PINVOKE.ToneGenerator_playDigits__SWIG_0(swigCPtr, ToneDigitVector.getCPtr(digits), loop);
    if (pjsua2PINVOKE.SWIGPendingException.Pending) throw pjsua2PINVOKE.SWIGPendingException.Retrieve();
  }

  public void playDigits(ToneDigitVector digits) {
    pjsua2PINVOKE.ToneGenerator_playDigits__SWIG_1(swigCPtr, ToneDigitVector.getCPtr(digits));
    if (pjsua2PINVOKE.SWIGPendingException.Pending) throw pjsua2PINVOKE.SWIGPendingException.Retrieve();
  }

  public ToneDigitMapVector getDigitMap() {
    ToneDigitMapVector ret = new ToneDigitMapVector(pjsua2PINVOKE.ToneGenerator_getDigitMap(swigCPtr), true);
    if (pjsua2PINVOKE.SWIGPendingException.Pending) throw pjsua2PINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public void setDigitMap(ToneDigitMapVector digit_map) {
    pjsua2PINVOKE.ToneGenerator_setDigitMap(swigCPtr, ToneDigitMapVector.getCPtr(digit_map));
    if (pjsua2PINVOKE.SWIGPendingException.Pending) throw pjsua2PINVOKE.SWIGPendingException.Retrieve();
  }

}
