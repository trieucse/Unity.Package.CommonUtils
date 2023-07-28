#ifndef __DEBUG_MESSAGES__
#define __DEBUG_MESSAGES__

// IMPORTANT!!!
// Put next 2 lines in the compute shader you want to debug and use ComputeShaderKernel.Dispatch(debug:true) to enable debug mode
// #pragma multi_compile_local __ ENABLE_DEBUG_MESSAGES
// #include "Packages/com.trackman.commonutils/Shaders/DebugMessages.hlsl"

#if ENABLE_DEBUG_MESSAGES
struct DebugMessage
{
    uint step;
    float4 values;
};

AppendStructuredBuffer<DebugMessage> debugMessages;

void Debug(uint step, float4 values)
{
    DebugMessage debug;
    debug.step = step;
    debug.values = values;
    debugMessages.Append(debug);
}
#else
void Debug(uint step, float4 values) 
{
    // Empty function to avoid error about struct buffer not set
}
#endif

#endif // __DEBUG_MESSAGES__