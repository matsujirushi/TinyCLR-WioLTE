#include "LedBlink.h"

static const TinyCLR_Interop_MethodHandler methods[] = {
    nullptr,
    nullptr,
    nullptr,
    nullptr,
    nullptr,
    Interop_LedBlink_LedBlink_WioLTE::LedSetRGB___VOID__U1__U1__U1,
    nullptr,
};

const TinyCLR_Interop_Assembly Interop_LedBlink = {
    "LedBlink",
    0xFEE4117D,
    methods
};
