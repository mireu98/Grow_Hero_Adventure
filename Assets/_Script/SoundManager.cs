using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public AudioMixer mainMixer;
    public Slider bgmSlider;
    public Slider sfxSlider;
    public GameObject SoundPanel;

    private void Start()
    {
        float bgmVol = PlayerPrefs.GetFloat("BGMVolume", 1f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 1f);

        // 2. 슬라이더 위치 초기화
        bgmSlider.value = bgmVol;
        sfxSlider.value = sfxVol;

        // 3. 실제 믹서에 적용
        SetBGMVolume(bgmVol);
        SetSFXVolume(sfxVol);
    }

    // BGM 슬라이더에 연결할 함수
    public void SetBGMVolume(float volume)
    {
        // volume이 0일 때는 확실하게 -80dB(음소거)로 보냅니다.
        if (volume <= 0.0001f)
        {
            mainMixer.SetFloat("BGMVolume", -80f);
        }
        else
        {
            mainMixer.SetFloat("BGMVolume", Mathf.Log10(volume) * 40f);
        }
    }

    // SFX 슬라이더에 연결할 함수
    public void SetSFXVolume(float volume)
    {
        if (volume <= 0.0001f)
        {
            mainMixer.SetFloat("SFXVolume", -80f);
        }
        else
        {
            mainMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20f);
        }
    }

    // Save 버튼에 연결할 함수
    public void SaveSoundSettings()
    {
        PlayerPrefs.SetFloat("BGMVolume", bgmSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", sfxSlider.value);
        PlayerPrefs.Save();
        SoundPanel.SetActive(false);
    }

    public void OpenSoundMenu()
    {
        SoundPanel.SetActive(true);
    }
}