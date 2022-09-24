#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

using UnityEngine.Rendering;

namespace MyScripts
{
    /// <summary>
    /// Configure HighFidelity URP Settings during import.
    /// </summary>
    public class HighFidelitySetProcessor : AssetPostprocessor
    {
        private static bool highFidelitySet = false;

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if(!highFidelitySet) 
            {
                var urpHighFidelity = Resources.Load<RenderPipelineAsset>("URP-HighFidelity");

                if (urpHighFidelity != null)
                {
                    GraphicsSettings.defaultRenderPipeline = urpHighFidelity;
                    QualitySettings.renderPipeline = urpHighFidelity;
                    highFidelitySet = true;
                }
            }
        }
    }
}
#endif