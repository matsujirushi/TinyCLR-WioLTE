#include "LedBlink.h"

static const TinyCLR_Interop_MethodHandler methods[] = {
    nullptr,
    nullptr,
    Interop_LedBlink_LedBlink_WioLTE::LedSetRGB___VOID__U1__U1__U1,
    nullptr,
};

const TinyCLR_Interop_Assembly Interop_LedBlink = {
    "LedBlink",
    0x83BD500B,
    methods
};
