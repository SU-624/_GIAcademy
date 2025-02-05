using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RankPanel : MonoBehaviour
{
    [SerializeField] private PopUpUI m_PopUpRankPanel;
    [SerializeField] private Image m_FadeInOutImage;
    [SerializeField] private TextMeshProUGUI m_SpeechBubbleText;
    [SerializeField] private GameObject m_SpeechBubbleObj;
    [SerializeField] private GameObject m_Rank1stEffectObj;
    [SerializeField] private GameObject m_CurtainObj;
    [SerializeField] private Animator m_CurtainAni;

    [Header("관중들 애니메이션")]
    [SerializeField] private Animator m_CrowdUp1;
    [SerializeField] private Animator m_CrowdUp2;
    [SerializeField] private Animator m_CrowdDown1;
    [SerializeField] private Animator m_CrowdDown2;

    public GameObject CurtainObj { get { return m_CurtainObj; } set { m_CurtainObj = value; } }
    public Image FadeInOutImage { get { return m_FadeInOutImage; } set { m_FadeInOutImage = value; } }

    /// fade 효과를 위한 변수들
    private Color _fadeOutImageColor;
    private Color _fadeInImageColor;
    private float m_FadeTime = 2f;

    public void InitFadeImage()
    {
        _fadeOutImageColor = m_FadeInOutImage.color;
        _fadeOutImageColor.a = 1f;
    }

    public void PopUpRankPanel()
    {
        m_FadeInOutImage.color = _fadeOutImageColor;

        m_PopUpRankPanel.TurnOnUI();

        StartCoroutine(FadeOutPanel());
    }

    public void Set1stRankEffect(bool _isActive)
    {
        m_Rank1stEffectObj.SetActive(_isActive);
    }

    public void PopOffRankPanel()
    {
        StartCoroutine(FadeInPanel());
    }

    IEnumerator FadeOutPanel()
    {
        while (m_FadeInOutImage.color.a > 0f)
        {
            m_FadeInOutImage.color = new Color(m_FadeInOutImage.color.r, m_FadeInOutImage.color.g, m_FadeInOutImage.color.b, m_FadeInOutImage.color.a - (Time.unscaledDeltaTime / m_FadeTime));
            yield return null;
        }
    }

    public IEnumerator FadeInPanel()
    {
        _fadeInImageColor.a = 0f;
        m_FadeInOutImage.color = _fadeInImageColor;

        while (1f > m_FadeInOutImage.color.a)
        {
            m_FadeInOutImage.color = new Color(m_FadeInOutImage.color.r, m_FadeInOutImage.color.g, m_FadeInOutImage.color.b, m_FadeInOutImage.color.a + (Time.unscaledDeltaTime / m_FadeTime));
            yield return null;
        }
    }

    public void SetSpeechBubbleText(string _text)
    {
        m_SpeechBubbleText.text = _text;
    }

    public void SetSpeechBubbleActive(bool _flag)
    {
        m_SpeechBubbleObj.SetActive(_flag);
    }

    public void PlayCurtenAnimation()
    {
        m_CurtainAni.Play("CurtainAnim", 0, 0f);
    }
}
