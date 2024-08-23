using UnityEngine;

[ExecuteInEditMode]

public class USBReplacementController : MonoBehaviour
{
    public Shader m_replacementShader;

    private void OnEnable()
    {
        if (m_replacementShader != null)
        {
            GetComponent<Camera>().SetReplacementShader(
                m_replacementShader, "RenderType");
        }
    }

    private void OnDisable()
    {
        GetComponent<Camera>().ResetReplacementShader();
    }
}