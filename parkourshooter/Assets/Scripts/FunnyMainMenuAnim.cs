using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FunnyMainMenuAnim : MonoBehaviour
{
    public TextMeshProUGUI subtitle;
    public AudioSource sounds, music;
    public AudioClip beepboop, backspace, ahem, typing, menumusic, buttonhoversound, buttonclicksound, startbuttonclick, quitbuttonclick;
    public float fontsize;
    public GameObject blackoutSquare;
    AsyncOperation damien;
    public GameObject startbutton;

    void Start()
    {
        StartCoroutine(nameof(writingthething));
        LoadScene();
    }

    IEnumerator LoadScene()
    {
        damien = SceneManager.LoadSceneAsync("Level1");
        damien.allowSceneActivation = false;
        while(damien.progress < 80)
        {
            startbutton.GetComponent<Button>().enabled = false;
        }

        startbutton.GetComponent<Button>().enabled = true;
        yield return null;
    }

    public void ShowCredits()
    {
        //show credits
    }

    IEnumerator FadeToBlack()
    {
        sounds.PlayOneShot(quitbuttonclick);
        blackoutSquare.SetActive(true);
        float fadeAmount;
        Color objectColor = blackoutSquare.GetComponent<Image>().color;
        while (blackoutSquare.GetComponent<Image>().color.a < 1)
        {
            fadeAmount = objectColor.a + (5 * Time.deltaTime);
            objectColor = new Color(objectColor.r, objectColor.g, objectColor.b, fadeAmount);
            blackoutSquare.GetComponent<Image>().color = objectColor;
            yield return null;
        }

        yield return new WaitForSeconds(1);
        Application.Quit();
    }

    public static class FadeAudioSource
    {
        public static IEnumerator StartFade(AudioSource audioSource, float duration, float targetVolume)
        {
            float currentTime = 0;
            float start = audioSource.volume;
            while (currentTime < duration)
            {
                currentTime += Time.deltaTime;
                audioSource.volume = Mathf.Lerp(start, targetVolume, currentTime / duration);
                yield return null;
            }
            yield break;
        }
    }

    // need to properly figure out how the autosize thing works so i can autosize the text based on the full length of the subtitle and then keep it there so that the size doesnt change when typing
    public void ButtonHover()
    {
        sounds.PlayOneShot(buttonhoversound);
    }
    public void ButtonSelect()
    {
        sounds.PlayOneShot(buttonclicksound);
    }
    public void StartButtonSelect()
    {

    }

    IEnumerator StartButton()
    {
        sounds.PlayOneShot(startbuttonclick);
        StartCoroutine(nameof(FadeToBlack));
        StartCoroutine(FadeAudioSource.StartFade(music, 2, 0));
        yield return new WaitForSeconds(5);
        damien.allowSceneActivation = true;

    }
    public void QuitButtonSelect()
    {
        StartCoroutine(FadeAudioSource.StartFade(music, 2, 0));
        StartCoroutine(nameof(FadeToBlack));
    }

    IEnumerator writingthething() // im writing this while sick. hopefully i clean this script before publishing
    {
        yield return new WaitForEndOfFrame();
        sounds.PlayOneShot(beepboop);
        subtitle.text = "p";
        yield return new WaitForSeconds(0.15f);
        subtitle.text = "pa";
        yield return new WaitForSeconds(0.15f);
        subtitle.text = "par";
        yield return new WaitForSeconds(0.15f);
        subtitle.text = "park";
        yield return new WaitForSeconds(0.15f);
        subtitle.text = "parko";
        yield return new WaitForSeconds(0.15f);
        subtitle.text = "parkou";
        yield return new WaitForSeconds(0.15f);
        subtitle.text = "parkour";
        yield return new WaitForSeconds(0.15f);
        subtitle.text = "parkour ";
        yield return new WaitForSeconds(0.15f);
        subtitle.text = "parkour s";
        yield return new WaitForSeconds(0.15f);
        subtitle.text = "parkour sh";
        yield return new WaitForSeconds(0.15f);
        subtitle.text = "parkour sho";
        yield return new WaitForSeconds(0.15f);
        subtitle.text = "parkour shoo";
        yield return new WaitForSeconds(0.15f);
        subtitle.text = "parkour shoot";
        yield return new WaitForSeconds(0.15f);
        subtitle.text = "parkour shoote";
        yield return new WaitForSeconds(0.15f);
        subtitle.text = "parkour shooter";
        yield return new WaitForSeconds(1);
        sounds.PlayOneShot(ahem);
        yield return new WaitForSeconds(0.8f);
        sounds.PlayOneShot(backspace);
        subtitle.text = "parkour shoote";
        yield return new WaitForSeconds(0.142f);
        subtitle.text = "parkour shoot";
        yield return new WaitForSeconds(0.142f);
        subtitle.text = "parkour shoo";
        yield return new WaitForSeconds(0.142f);
        subtitle.text = "parkour sho";
        yield return new WaitForSeconds(0.142f);
        subtitle.text = "parkour sh";
        yield return new WaitForSeconds(0.142f);
        subtitle.text = "parkour s";
        yield return new WaitForSeconds(0.142f);
        subtitle.text = "parkour ";
        yield return new WaitForSeconds(0.25f);
        sounds.PlayOneShot(typing);
        yield return new WaitForSeconds(0.133f);
        subtitle.text = "parkour d";
        yield return new WaitForSeconds(0.126f);
        subtitle.text = "parkour di";
        yield return new WaitForSeconds(0.068f);
        subtitle.text = "parkour dis";
        yield return new WaitForSeconds(0.121f);
        subtitle.text = "parkour disp";
        yield return new WaitForSeconds(0.086f);
        subtitle.text = "parkour dispa";
        yield return new WaitForSeconds(0.103f);
        subtitle.text = "parkour dispat";
        yield return new WaitForSeconds(0.272f);
        subtitle.text = "parkour dispatc";
        yield return new WaitForSeconds(0.127f);
        subtitle.text = "parkour dispatch";
        yield return new WaitForSeconds(0.139f);
        subtitle.text = "parkour dispatche";
        yield return new WaitForSeconds(0.124f);
        subtitle.text = "parkour dispatcher";
        yield return new WaitForSeconds(0.3f);
        music.time = 27.588f;
        music.loop = true;
        music.Play();
    }

}
