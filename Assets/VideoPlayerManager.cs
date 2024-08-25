using UnityEngine;
using UnityEngine.Video;


// Game logic for when to play each video clip (tutorial videos)
public class VideoPlayerManager : MonoBehaviour
{
    public VideoClip videoClip1;
    public VideoClip videoClip2;
    public VideoClip videoClip3;

    private VideoPlayer videoPlayer;
    private bool[] videoPlayed;

    private void Awake()
    {
        videoPlayed = new bool[3];
    }

    private void Update()
    {
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            return; // If a video is already playing, do nothing
        }

        // Play the video based on the current phase of the game
        switch (GameManager.currentPhase)
        {
            case (int)Phase.SafetyPadding:
                if (!videoPlayed[0])
                {
                    PlayVideo(videoClip1, 0);
                }
                break;
            case (int)Phase.Preparation1:
                if (!videoPlayed[1] && GameManager.hasHands)
                {
                    PlayVideo(videoClip2, 1);
                }
                break;
            case (int)Phase.Preparation2:
                if (!videoPlayed[2] && GameManager.hasPendulum)
                {
                    PlayVideo(videoClip3, 2);
                }
                break;
        }
    }

    // Play only one video at a time
    private void PlayVideo(VideoClip clip, int clipIndex)
    {
        if (videoPlayer != null)
        {
            Destroy(videoPlayer);
        }

        CreateVideoPlayer();
        videoPlayer.clip = clip;
        videoPlayer.Play();
        videoPlayed[clipIndex] = true;
    }

    // Add and setup a video player component to the attached game object
    private void CreateVideoPlayer()
    {
        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.loopPointReached += OnVideoEnd;
    }

    // Remove the video player component when the video ends
    private void OnVideoEnd(VideoPlayer vp)
    {
        Destroy(vp);
        videoPlayer = null;
    }
}
