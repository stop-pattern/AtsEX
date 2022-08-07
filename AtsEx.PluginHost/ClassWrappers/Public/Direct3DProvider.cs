﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using SlimDX.Direct3D9;

using Automatic9045.AtsEx.PluginHost.BveTypes;

namespace Automatic9045.AtsEx.PluginHost.ClassWrappers
{
    /// <summary>
    /// Direct3D の機能を利用するためのメンバーを提供します。
    /// </summary>
    public class Direct3DProvider : ClassWrapperBase
    {
        [InitializeClassWrapper]
        private static void Initialize(BveTypeSet bveTypes)
        {
            ClassMemberSet members = bveTypes.GetClassInfoOf<Direct3DProvider>();

            InstanceGetMethod = members.GetSourcePropertyGetterOf(nameof(Instance));

            DeviceGetMethod = members.GetSourcePropertyGetterOf(nameof(Device));
            PresentParametersGetMethod = members.GetSourcePropertyGetterOf(nameof(PresentParameters));
            Direct3DGetMethod = members.GetSourcePropertyGetterOf(nameof(Direct3D));
        }

        /// <summary>
        /// <see cref="Direct3DProvider"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="src">ラップするオリジナル オブジェクト。</param>
        protected Direct3DProvider(object src) : base(src)
        {
        }

        /// <summary>
        /// オリジナル オブジェクトからラッパーのインスタンスを生成します。
        /// </summary>
        /// <param name="src">ラップするオリジナル オブジェクト。</param>
        /// <returns>オリジナル オブジェクトをラップした <see cref="Direct3DProvider"/> クラスのインスタンス。</returns>
        [CreateClassWrapperFromSource]
        public static Direct3DProvider FromSource(object src) => src is null ? null : new Direct3DProvider(src);

        private static MethodInfo InstanceGetMethod;
        /// <summary>
        /// <see cref="Direct3DProvider"/> クラスのインスタンスを取得します。
        /// </summary>
        public static Direct3DProvider Instance => FromSource(InstanceGetMethod.Invoke(null, null));

        private static MethodInfo DeviceGetMethod;
        /// <summary>
        /// <see cref="SlimDX.Direct3D9.Device"/> を取得します。
        /// </summary>
        public Device Device => (Device)DeviceGetMethod.Invoke(Src, null);

        private static MethodInfo PresentParametersGetMethod;
        /// <summary>
        /// <see cref="Device"/> の生成に使用した <see cref="SlimDX.Direct3D9.PresentParameters"/> を取得します。
        /// </summary>
        public PresentParameters PresentParameters => (PresentParameters)PresentParametersGetMethod.Invoke(Src, null);

        private static MethodInfo Direct3DGetMethod;
        /// <summary>
        /// <see cref="SlimDX.Direct3D9.Direct3D"/> を取得します。
        /// </summary>
        public Direct3D Direct3D => (Direct3D)Direct3DGetMethod.Invoke(Src, null);
    }
}