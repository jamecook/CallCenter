//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 4.0.1
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------


public enum pjmedia_dir {
  PJMEDIA_DIR_NONE = 0,
  PJMEDIA_DIR_ENCODING = 1,
  PJMEDIA_DIR_CAPTURE = PJMEDIA_DIR_ENCODING,
  PJMEDIA_DIR_DECODING = 2,
  PJMEDIA_DIR_PLAYBACK = PJMEDIA_DIR_DECODING,
  PJMEDIA_DIR_RENDER = PJMEDIA_DIR_DECODING,
  PJMEDIA_DIR_ENCODING_DECODING = 3,
  PJMEDIA_DIR_CAPTURE_PLAYBACK = PJMEDIA_DIR_ENCODING_DECODING,
  PJMEDIA_DIR_CAPTURE_RENDER = PJMEDIA_DIR_ENCODING_DECODING
}
