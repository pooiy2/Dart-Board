using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Windows.Kinect;

public class Calibration : MonoBehaviour
{
    //Pass Ins
    public GameObject TopLeft;
    public MeasureDepth mMeasureDepth;
    public MultiSourceManager mMultiSource;


    //Variables
    public int count = 0;
    public int start = 0;
    Color[] img = null;
    RawImage square;

    //Resulting list consisting of 1's at screen position
    public List<int> corners;


    // Start is called before the first frame update
    void Start()
    {
        corners = new List<int>();
        square = TopLeft.GetComponent<RawImage>();
    }

    // Update is called once per frame
    void Update()
    {
        //At different intervals work on calibrating screen
        if (start==0){ //Waits while kinect camera feed is not active
            img = mMultiSource.GetColorTexture().GetPixels();
            if (Math.Abs(img[0].r-0.8039216) > 0.05 & Math.Abs(img[0].r-0.8039216) > 0.05 & Math.Abs(img[0].r-0.8039216) > 0.05){
                start+=1;
            }
        } else if (start==1){
            Texture2D saveImg = mMultiSource.GetColorTexture();
            SaveTextureAsPNG(saveImg, "C:/Users/ldkea/Desktop/imgraw.png");
            start++;
        } else if (start==2){
            Calibrate();
            Fiducial();
            Texture2D saveImg = mMultiSource.GetColorTexture();
            saveImg.SetPixels(img);
            SaveTextureAsPNG(saveImg, "C:/Users/ldkea/Desktop/imgqr.png");
        }
        /*
        if (start==1){    //Initialize list of ints to 1 
            for (int i = 0; i < mMultiSource.ColorHeight * mMultiSource.ColorWidth; i++){
                corners.Add(1);
            }
            start++;
            ChangeColor(Color.black); //Change the color of the screen to black
        } else if (start==50) {    //Change all ints in list to 0 where the camera does not pick up the color black
            Calibrate(0, 0, 0, .85);
            start++;
            ChangeColor(Color.white);
            //Save image with blue over the parts that the camera thinks is black
            img = mMultiSource.GetColorTexture().GetPixels();
            Texture2D saveImg = mMultiSource.GetColorTexture();
            for (int i = 0; i < mMultiSource.ColorHeight * mMultiSource.ColorWidth; i++){
                if (corners[i]==1){
                    img[i] = Color.blue;
                }
            }
            saveImg.SetPixels(img);
            SaveTextureAsPNG(saveImg, "C:/Users/ldkea/Desktop/imgbefore.png");

        } else if (start==100) { //Change all ints in list to 0 where canera does not pick up the color white
            Calibrate(1, 1, 1, .12);
        //Save image with blue over the parts that the camera first thought was black and now is white (should only include the screen)
            img = mMultiSource.GetColorTexture().GetPixels();
            Texture2D saveImg = mMultiSource.GetColorTexture();
            for (int i = 0; i < mMultiSource.ColorHeight * mMultiSource.ColorWidth; i++){
                if (corners[i]==1){
                    img[i] = Color.red;
                    count+=1;
                }
            }
            //Save image
            saveImg.SetPixels(img);
            SaveTextureAsPNG(saveImg, "C:/Users/ldkea/Desktop/imgafter.png");
            Destroy(TopLeft); //Remove image used to change screen color once calibration is complete
            start++;
        } else if (start>0 & start<100){
            start++;
        }
        */
    }

    public void SaveTextureAsPNG(Texture2D _texture, string _fullPath)
         {
             //Saves Textures to a PNG file
             byte[] _bytes =_texture.EncodeToPNG();
             System.IO.File.WriteAllBytes(_fullPath, _bytes);
             Debug.Log(_bytes.Length/1024  + "Kb was saved as: " + _fullPath);
         }

    void Calibrate(double e = .995){
        //Uses the camera feed to find qr square
        img = mMultiSource.GetColorTexture().GetPixels();
        //Changes each pixel to either black or white depending on threshold e
        for (int i = 0; i < mMultiSource.ColorHeight * mMultiSource.ColorWidth; i++){
            if (img[i].r < e & img[i].g < e & img[i].b < e){
                img[i] = Color.black;
            } else {
                img[i] = Color.white;
            }
        }
    }
    void Fiducial(){
        //Finds the qr square
        int[] counts;
        Vector2[] ends = new Vector2[5];
        int counting = 0;
        int i;
        for (int x=0;  x<mMultiSource.ColorHeight; x++){
            counts = new int[5];
            ends = new Vector2[5];
            counting = 0;
            for (int y=0; y<mMultiSource.ColorWidth; y++){
                i = x * mMultiSource.ColorHeight + mMultiSource.ColorWidth;
                if (new int[] {0, 2, 4}.contains(counting)){ //Count black pixels
                    if (img[i]==Color.black){
                        counts[counting]++;
                    } else if (counting!=4){
                        ends[counting].x = x;
                        ends[counting].y = y;
                        counting++;
                        counts[counting]++;
                    } else if (counting==4){ //If you have passed five intervals check if it is the qr marker
                        ends[counting].x = x;
                        ends[counting].y = y;
                        counting = Verify(count); //Check if rations match
                        if (counting==4){ //If they match return the relevant values (probably start location)
                            // return Some value to use later
                        } else if (counting==2){
                            ends = new int[] {ends[2], ends[3], ends[4], 0, 0};
                            counts = new int[] {counts[2], counts[3], counts[4], 0, 0};
                        }
                        //Else change the count accordingly
                    }
                } else if (new int[] {1,3}.contains(counting)){ //Count white pixels
                    if (img[i]==Color.white){
                        counts[counting]++;
                    } else {
                        ends[counting].x = x;
                        ends[counting].y = y;
                        counting++;
                        counts[counting]++;
                    }
                }
            }
        }
    }

    int Verify(int[] counts){
        //Takes a list of pixel counts and returns true if the ratios match the qr marker

    }

    void ChangeColor(Color color){
        //Changes the color of the screen
        square.color = color;
    }
}