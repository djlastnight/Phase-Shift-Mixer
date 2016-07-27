namespace VuShaderEffect
{
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Effects;

    public class VuEffect : ShaderEffect
    {
        // Brush-valued properties turn into sampler-property in the shader.
        // This helper sets "ImplicitInput" as the default, meaning the default
        // sampler is whatever the rendering of the element it's being applied to is.
        public static readonly DependencyProperty InputProperty =
            ShaderEffect.RegisterPixelShaderSamplerProperty(
            "Input",
            typeof(VuEffect),
            0);

        // Scalar-valued properties turn into shader constants with the register
        // number sent into PixelShaderConstantCallback().
        public static readonly DependencyProperty LevelProperty =
            DependencyProperty.Register(
            "Level",
            typeof(float),
            typeof(VuEffect),
            new UIPropertyMetadata(0.0f, PixelShaderConstantCallback(0)));

        private static PixelShader pixelShader = new PixelShader();

        static VuEffect()
        {
            pixelShader.UriSource = Global.MakePackUri("vueffect.ps");
        }

        public VuEffect()
        {
            this.PixelShader = pixelShader;

            // Update each DependencyProperty that's registered with a shader register.  This
            // is needed to ensure the shader gets sent the proper default value.
            this.UpdateShaderValue(InputProperty);
            this.UpdateShaderValue(LevelProperty);
        }

        public Brush Input
        {
            get { return (Brush)this.GetValue(InputProperty); }
            set { this.SetValue(InputProperty, value); }
        }

        public float Level
        {
            get { return (float)this.GetValue(LevelProperty); }
            set { this.SetValue(LevelProperty, value); }
        }
    }
}