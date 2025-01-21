using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Unity.Jobs;

public class MorphTargetController : MonoBehaviour
{
    public SkinnedMeshRenderer skinnedMeshRenderer = null;
    // private string previousText = "";

   private readonly Dictionary<string, Dictionary<string, float>> emotionMappings = new Dictionary<string, Dictionary<string, float>>
    {
        {
            "joy", new Dictionary<string, float>
            {
                { "mouthSmile", 0.6f },
                { "mouthSmileLeft", 0.35f },
                { "mouthSmileRight", 0.35f },
                { "mouthOpen", 0.3f },
                { "cheekSquintLeft", 0.6f },
                { "cheekSquintRight", 0.6f },
                { "eyeSquintLeft", 0.4f },
                { "eyeSquintRight", 0.4f },
                { "browOuterUpLeft", 0.3f },
                { "browOuterUpRight", 0.3f }
            }
        },
        {
            "sadness", new Dictionary<string, float>
            {
                { "browDownLeft", 0.75f },
                { "browDownRight", 0.75f },
                { "browInnerUp", 0.7f },
                { "mouthSmile", -0.4f },
                { "mouthLeft", -0.3f },
                { "mouthRight", -0.3f },
                { "eyeWideLeft", -0.2f },
                { "eyeWideRight", -0.2f },
                { "cheekPuff", -0.3f },
                { "jawOpen", 0.2f }
            }
        },
        {
            "anger", new Dictionary<string, float>
            {
                { "browDownLeft", 0.1f },
                { "browDownRight", 0.1f },
                { "browInnerUp", -0.1f },
                { "noseSneerLeft", 1f },
                { "noseSneerRight", 1f },
                { "jawForward", 0.7f },
                { "mouthPucker", 0.5f },
                { "cheekSquintLeft", 0.3f },
                { "cheekSquintRight", 0.3f },
                { "mouthSmile", -0.5f }
            }
        },
        {
            "fear", new Dictionary<string, float>
            {
                { "eyeWideLeft", 0.45f },
                { "eyeWideRight", 0.45f },
                { "mouthOpen", 0.25f },
                { "browOuterUpLeft", 0.5f },
                { "browOuterUpRight", 0.5f },
                { "browInnerUp", 0.6f },
                { "jawOpen", 0.1f },
                { "cheekSquintLeft", -0.2f },
                { "cheekSquintRight", -0.2f }
            }
        },
        {
            "surprise", new Dictionary<string, float>
            {
                { "mouthOpen", 0.25f },
                { "browOuterUpLeft", 0.45f },
                { "browOuterUpRight", 0.45f },
                { "browInnerUp", 0.45f },
                { "eyeWideLeft", 0.5f },
                { "eyeWideRight", 0.5f },
                { "cheekPuff", 0.3f },
                { "jawOpen", 0.1f }
            }
        }
    };



    void Start()
    {
        SetBlendShapeWeight("browInnerUp", 0.2f);
    }

    void Update()
    {
    }

    public void AdjustMorphTargets(JObject json)
    {
        try
        {
            JObject emotionData = json.GetValue("emotion_scores") as JObject;

            var primaryEmotion = emotionData.Properties()
                .OrderByDescending(e => (float)e.Value)
                .FirstOrDefault();

            if (primaryEmotion != null)
            {
                string emotionKey = primaryEmotion.Name;
                // float score = (float)primaryEmotion.Value * 100;
                float score = (float)primaryEmotion.Value;

                Debug.Log($"Primary emotion: {emotionKey} with score: {score}");
                ResetBlendShapes();

                ApplyEmotion(emotionKey, score);
            }
            else
            {
                Debug.LogWarning("No primary emotion found in the received data.");
            }
        }
        catch (Newtonsoft.Json.JsonReaderException jsonEx)
        {
            Debug.LogError($"JSON Parse Error: {jsonEx.Message}. Raw JSON: {json}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Exception in AdjustMorphTargets: {ex.Message}");
        }
    }

    private void ApplyEmotion(string emotion, float score)
    {
        if (emotionMappings.TryGetValue(emotion, out var morphMap))
        {
            foreach (var morph in morphMap)
            {
                float weight = morph.Value * (score / 100);
                SetBlendShapeWeight(morph.Key, weight);
                Debug.Log($"Applying {morph.Key} with weight: {weight}");
            }
        }
        else
        {
            Debug.LogWarning($"No mappings found for emotion: {emotion}");
        }
    }

    private void SetBlendShapeWeight(string morph, float weight)
    {
        int index = GetBlendShapeIndex(morph);
        if (index != -1)
        {
            skinnedMeshRenderer.SetBlendShapeWeight(index, Mathf.Clamp(weight, 0f, 1f));
        }
        else
        {
            Debug.LogWarning($"Blend shape for morph '{morph}' not found.");
        }
    }

    private int GetBlendShapeIndex(string morph)
    {
        switch (morph)
        {
            case "mouthOpen": return 0;      // Index for mouthOpen
            case "viseme_sil": return 1;     // Index for viseme_sil
            case "viseme_PP": return 2;      // Index for viseme_PP
            case "viseme_FF": return 3;      // Index for viseme_FF
            case "viseme_TH": return 4;      // Index for viseme_TH
            case "viseme_DD": return 5;      // Index for viseme_DD
            case "viseme_kk": return 6;      // Index for viseme_kk
            case "viseme_CH": return 7;      // Index for viseme_CH
            case "viseme_SS": return 8;      // Index for viseme_SS
            case "viseme_nn": return 9;      // Index for viseme_nn
            case "viseme_RR": return 10;     // Index for viseme_RR
            case "viseme_aa": return 11;     // Index for viseme_aa
            case "viseme_E": return 12;      // Index for viseme_E
            case "viseme_I": return 13;      // Index for viseme_I
            case "viseme_O": return 14;      // Index for viseme_O
            case "viseme_U": return 15;      // Index for viseme_U
            case "mouthSmile": return 16;    // Index for mouthSmile
            case "browDownLeft": return 17;  // Index for browDownLeft
            case "browDownRight": return 18; // Index for browDownRight
            case "browInnerUp": return 19;   // Index for browInnerUp
            case "browOuterUpLeft": return 20; // Index for browOuterUpLeft
            case "browOuterUpRight": return 21; // Index for browOuterUpRight
            case "eyeSquintLeft": return 22;  // Index for eyeSquintLeft
            case "eyeSquintRight": return 23; // Index for eyeSquintRight
            case "eyeWideLeft": return 24;    // Index for eyeWideLeft
            case "eyeWideRight": return 25;   // Index for eyeWideRight
            case "jawForward": return 26;      // Index for jawForward
            case "jawLeft": return 27;         // Index for jawLeft
            case "jawRight": return 28;        // Index for jawRight
            case "mouthPucker": return 29;     // Index for mouthPucker
            case "noseSneerLeft": return 30;   // Index for noseSneerLeft
            case "noseSneerRight": return 31;  // Index for noseSneerRight
            case "mouthLeft": return 32;       // Index for mouthLeft
            case "mouthRight": return 33;      // Index for mouthRight
            case "eyeLookDownLeft": return 34; // Index for eyeLookDownLeft
            case "eyeLookDownRight": return 35; // Index for eyeLookDownRight
            case "eyeLookUpLeft": return 36;   // Index for eyeLookUpLeft
            case "eyeLookUpRight": return 37;  // Index for eyeLookUpRight
            case "eyeLookInLeft": return 38;   // Index for eyeLookInLeft
            case "eyeLookInRight": return 39;  // Index for eyeLookInRight
            case "eyeLookOutLeft": return 40;  // Index for eyeLookOutLeft
            case "eyeLookOutRight": return 41; // Index for eyeLookOutRight
            case "cheekPuff": return 42;       // Index for cheekPuff
            case "cheekSquintLeft": return 43; // Index for cheekSquintLeft
            case "cheekSquintRight": return 44; // Index for cheekSquintRight
            case "jawOpen": return 45;         // Index for jawOpen
            case "mouthSmileLeft": return 46;  // Index for mouthSmileLeft
            case "mouthSmileRight": return 47; // Index for mouthSmileRight
            case "tongueOut": return 48;       // Index for tongueOut
            case "eyeBlinkLeft": return 49;    // Index for eyeBlinkLeft
            case "eyeBlinkRight": return 50;   // Index for eyeBlinkRight
            case "eyesClosed": return 51;      // Index for eyesClosed
            case "eyesLookUp": return 52;      // Index for eyesLookUp
            case "eyesLookDown": return 53;    // Index for eyesLookDown
            default: return -1;                 // Not found
        }
    }

    private void ResetBlendShapes()
    {
        if (skinnedMeshRenderer == null)
        {
            Debug.LogError("skinnedMeshRenderer is null. Cannot reset blend shapes.");
            return; 
        }

        int blendShapeCount = skinnedMeshRenderer.sharedMesh.blendShapeCount;
        for (int i = 0; i < blendShapeCount; i++)
        {
            skinnedMeshRenderer.SetBlendShapeWeight(i, 0);
        }
        SetBlendShapeWeight("browInnerUp", 0.2f);
    }
}