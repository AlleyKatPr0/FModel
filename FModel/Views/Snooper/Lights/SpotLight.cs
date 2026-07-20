using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Objects.Core.Misc;
using FModel.Views.Snooper.Shading;
using ImGuiNET;

namespace FModel.Views.Snooper.Lights;

public class SpotLight : Light
{
    public float Attenuation;
    public float InnerConeAngle;
    public float OuterConeAngle;

    public SpotLight(Texture icon, UObject spot) : base(icon, spot)
    {
        if (!spot.TryGetValue(out Attenuation, "SourceRadius", "AttenuationRadius"))
            Attenuation = 1.0f;

        Attenuation *= Constants.SCALE_DOWN_RATIO;
        InnerConeAngle = spot.GetOrDefault("InnerConeAngle", 50.0f);
        OuterConeAngle = spot.GetOrDefault("OuterConeAngle", InnerConeAngle + 10);
        if (OuterConeAngle < InnerConeAngle)
            InnerConeAngle = OuterConeAngle - 10;
    }

    public SpotLight(FGuid model, Texture icon, UObject parent, UObject spot, Transform transform) : base(model, icon, parent, spot, transform)
    {
        if (!spot.TryGetValue(out Attenuation, "AttenuationRadius", "SourceRadius"))
            Attenuation = 1.0f;

        Attenuation *= Constants.SCALE_DOWN_RATIO;
        InnerConeAngle = spot.GetOrDefault("InnerConeAngle", 50.0f);
        OuterConeAngle = spot.GetOrDefault("OuterConeAngle", InnerConeAngle + 10);
        if (OuterConeAngle < InnerConeAngle)
            InnerConeAngle = OuterConeAngle - 10;
    }

    private int _cachedIndex = -1;
    private string _uAttenuation;
    private string _uInnerConeAngle;
    private string _uOuterConeAngle;
    private string _uType;

    public override void Render(int i, Shader shader)
    {
        base.Render(i, shader);
        if (_cachedIndex != i)
        {
            _cachedIndex = i;
            _uAttenuation = $"uLights[{i}].Attenuation";
            _uInnerConeAngle = $"uLights[{i}].InnerConeAngle";
            _uOuterConeAngle = $"uLights[{i}].OuterConeAngle";
            _uType = $"uLights[{i}].Type";
        }
        shader.SetUniform(_uAttenuation, Attenuation);
        shader.SetUniform(_uInnerConeAngle, InnerConeAngle);
        shader.SetUniform(_uOuterConeAngle, OuterConeAngle);

        shader.SetUniform(_uType, 1);
    }

    public override void ImGuiLight()
    {
        base.ImGuiLight();
        SnimGui.Layout("Attenuation");ImGui.PushID(3);
        ImGui.DragFloat("", ref Attenuation, 0.1f);ImGui.PopID();
        SnimGui.Layout("Inner Cone Angle");ImGui.PushID(4);
        ImGui.DragFloat("", ref InnerConeAngle, 0.1f, 0.0f, 90.0f, "%.1f°");ImGui.PopID();
        SnimGui.Layout("Outer Cone Angle");ImGui.PushID(5);
        ImGui.DragFloat("", ref OuterConeAngle, 0.1f, 0.0f, 90.0f, "%.1f°");ImGui.PopID();
    }
}
