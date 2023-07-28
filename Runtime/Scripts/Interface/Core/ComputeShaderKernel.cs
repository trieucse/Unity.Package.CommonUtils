using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Rendering;

namespace Trackman.CourseTurf.Utility
{
    public sealed class ComputeShaderKernel : IDisposable
    {
#if UNITY_EDITOR
        public const string EnableDebugMessagesKeyword = "ENABLE_DEBUG_MESSAGES";
        public const string DebugMessagesBufferName = "debugMessages";
#endif

        #region Containers
        [StructLayout(LayoutKind.Sequential)]
        struct DebugMessage
        {
            #region Fields
            public int step;
            public Vector4 values;
            #endregion

            #region Properties
            public bool Valid => step > 0;
            #endregion

            #region Methods
            public override string ToString() => $"{step} : {values.x}, {values.y}, {values.z}, {values.w}";
            #endregion
        }
        #endregion

        #region Fields
        readonly ComputeShader computeShader;
        readonly int kernelID;

        readonly Dictionary<string, GraphicsBuffer> buffers = new();
        GraphicsBuffer debugMessagesBuffer;
        LocalKeyword? debugMessagesKeyword;
        #endregion

        #region Constructors
        public ComputeShaderKernel(ComputeShader computeShader, string kernelName)
        {
            this.computeShader = computeShader;
            kernelID = computeShader.FindKernel(kernelName);
            if (kernelID == -1)
                throw new ArgumentException($"Compute shader {computeShader.name} doesn't have kernel {kernelName} or compiled with errors.");
        }
        #endregion

        #region Methods
        public GraphicsBuffer CreateBuffer<T>(string name, GraphicsBuffer.Target target, int length) where T : struct
        {
            buffers.GetValueOrDefault(name)?.ReleaseSafe();
            GraphicsBuffer buffer = new GraphicsBuffer(target, length, UnsafeUtility.SizeOf<T>());
            AddBuffer(name, buffer);
            return buffer;
        }
        public void AddBuffer(string name, GraphicsBuffer buffer)
        {
            buffers[name] = buffer;
            computeShader.SetBuffer(kernelID, name, buffer);
        }
        public GraphicsBuffer CreateBufferWithData<T>(string name, GraphicsBuffer.Target target, T[] data) where T : struct
        {
            GraphicsBuffer buffer = CreateBuffer<T>(name, target, data.Length);
            buffer.SetData(data);
            debugMessagesKeyword = null;
            return buffer;
        }
        public void ReleaseBuffers()
        {
            buffers.Values.ForEach(x => x.ReleaseSafe());
            buffers.Clear();
            debugMessagesBuffer.ReleaseSafe();
        }
        public void SetExternalBuffer(string name, GraphicsBuffer buffer)
        {
            computeShader.SetBuffer(kernelID, name, buffer);
        }
        public void SetTexture(string name, RenderTexture texture, RenderTextureSubElement element)
        {
            computeShader.SetTexture(kernelID, name, texture, 0, element);
        }
        public uint GetThreadGroupSize(uint index)
        {
            uint[] size = new uint[3];
            computeShader.GetKernelThreadGroupSizes(kernelID, out size[0], out size[1], out size[2]);
            return size[index];
        }
        public void Dispatch(uint x, uint y, uint z, bool debug = false)
        {
            int CalcSize(uint length, uint index) => length == 1 ? 1 : Mathf.CeilToInt((float)length / GetThreadGroupSize(index));

            if (debug) BeginDebug();

            computeShader.Dispatch(kernelID, CalcSize(x, 0), CalcSize(y, 1), CalcSize(z, 2));

            if (debug) EndDebug();
        }
        public void DispatchIndirect(GraphicsBuffer args, uint offset, bool debug = false)
        {
            if (debug) BeginDebug();

            computeShader.DispatchIndirect(kernelID, args, offset);

            if (debug) EndDebug();
        }
        void IDisposable.Dispose() => ReleaseBuffers();
        #endregion

        #region Support Methods
        void BeginDebug()
        {
#if UNITY_EDITOR
            if (!debugMessagesKeyword.HasValue)
            {
                debugMessagesKeyword = computeShader.keywordSpace.FindKeyword(EnableDebugMessagesKeyword);
                if (!debugMessagesKeyword.Value.isValid) return;
            }

            computeShader.EnableKeyword(debugMessagesKeyword.Value);

            if (debugMessagesBuffer?.IsValid() != true)
                debugMessagesBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Append, 1024, UnsafeUtility.SizeOf<DebugMessage>());

            computeShader.SetBuffer(kernelID, DebugMessagesBufferName, debugMessagesBuffer);
            debugMessagesBuffer.SetData(Enumerable.Repeat<byte>(0, UnsafeUtility.SizeOf<DebugMessage>() * debugMessagesBuffer.count).ToArray());
            debugMessagesBuffer.SetCounterValue(0);
#endif
        }
        void EndDebug()
        {
#if UNITY_EDITOR
            if (!debugMessagesKeyword.HasValue || !debugMessagesKeyword.Value.isValid) return;

            computeShader.DisableKeyword(debugMessagesKeyword.Value);

            DebugMessage[] messages = new DebugMessage[debugMessagesBuffer.count];
            debugMessagesBuffer.GetData(messages);
            foreach (DebugMessage message in messages.TakeWhile(x => x.Valid))
                Debug.Log(message);
#endif
        }
        #endregion
    }
}