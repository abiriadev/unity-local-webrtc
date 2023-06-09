using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Unity.WebRTC;

public class X : MonoBehaviour {
    [SerializeField]
    private RawImage rawl, rawr;
    
    private RTCPeerConnection pc1, pc2;
    private WebCamTexture wct;
    private VideoStreamTrack vst;
    
    private void Awake() {
        WebRTC.Initialize();
    }

    private void OnDestroy() {
        wct.Stop();

        pc1.Close();
        pc2.Close();

        WebRTC.Dispose();
    }

    void Start() {
        wct = new WebCamTexture();
        
        wct.Play();
        rawl.texture = wct;

        vst = new VideoStreamTrack(wct);
        GetComponent<Renderer>().material.mainTexture = vst.Texture;

        pc1 = new RTCPeerConnection();
        pc2 = new RTCPeerConnection();

        pc1.OnIceCandidate += cand => pc2.AddIceCandidate(cand);
        pc2.OnIceCandidate += cand => pc1.AddIceCandidate(cand);

        pc2.OnTrack += OnTrack;
        pc1.OnNegotiationNeeded = () => { StartCoroutine(sig()); };

        pc1.AddTrack(vst);

        StartCoroutine(WebRTC.Update());
    }

    void Update() {}

    IEnumerator sig() {
        var offer = pc1.CreateOffer();
        yield return offer;
        var o = offer.Desc;
        yield return pc1.SetLocalDescription(ref o);
        yield return pc2.SetRemoteDescription(ref o);
        var answer = pc2.CreateAnswer();
        yield return answer;
        var a = answer.Desc;
        yield return pc2.SetLocalDescription(ref a);
        yield return pc1.SetRemoteDescription(ref a);
    }

    void OnTrack(RTCTrackEvent e) {
        if (e.Track is VideoStreamTrack track) {
            Debug.Log($"vid stream added");
            
            track.OnVideoReceived += (Texture r) => {
                Debug.Log("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
                rawr.texture = r;
            };
        }
    }
}
