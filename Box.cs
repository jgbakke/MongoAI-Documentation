using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour {

    // AI Properties
    public float red = 0.2f;
    public float green = 0.2f;
    public float blue = 0.2f;

    // You need a list of the properties you want controlled for AI
    public List<string> geneticProperties = new List<string>();
    
    // Just doing this as a shorthand to save time later
    private string className;
    
    // This is only for our example
    public Color desiredColor;
    public bool prepopulate = false;

    public float timeInterval = 2.0f;
    private float lastTime;
    private float startTime;

    private SpriteRenderer spriteRenderer;

    private bool reachGoal = false;
    
    // This project comes with 2 'modes'
    // 1) Live updates
    // These are SLOW; you are fetching data from a remote server and running a genetic algorithm
    // These are very good for evolving over time but should usually only be used during loading
    // Using during gameplay may cause performance issues
    // 2) Cached Updates
    // These are FAST; you cache a number of precomputed values, so when you need a new value
    // you can just call PopulateFromCache
    // This method cannot yield evolution as quickly because you are not recalculating the heuristic
    // and getting live updates from the DB.
    // This is a usually better alternative to use in the middle of gameplay if you are fine with
    // slower evolution, but less performance issues.
    // RUNNING
    // Go to The Game Editor and Check Prepoulate if you want to use caching, leave it unchecked otherwise


    void Start() {
        // Setup class variables
        className = GetType().Name; // The name of this class stored for the AI
        if(!prepopulate){MongoAI.manager.ClearData(className);} // We don't want it deleted if we are about to prepopulate
        lastTime = Time.time;
        startTime = Time.time;
        spriteRenderer = GetComponent<SpriteRenderer>();

        // MongoAI Stuff
        if(prepopulate){MongoAI.manager.CacheData(className, 40);} // Only bother caching if we need to
        UpdateProperties();
    }

    private void Update() {
        if (Time.time - lastTime > timeInterval && !reachGoal) {
            SaveAIData();
            SubmitAIData();
            UpdateProperties();
            lastTime = Time.time;
        }
    }
    private void UpdateProperties() {
        if (!prepopulate) {
            MongoAI.manager.PopulateProperties(this, geneticProperties, className, 2, 0.25f, false);
        } else if (prepopulate && Input.GetAxis("Jump") > 0.2f) {
            // GetAxis(Jump) means "When space is pressed"
            // What this does is if we are in cachine mode and press space, change to color from a cached chromosome
            MongoAI.manager.PopulateFromCache(this, geneticProperties, className, 2, 0.25f, false);
        }
        
        SetColor();
    }

    private void SetColor() {
        spriteRenderer.color = new Color(LimitColor(red), LimitColor(green), LimitColor(blue));
    }

    private float LimitColor(float val) {
        return val > 1.0f ? 1.0f : val;
    }
    
    private void SubmitAIData() {
        MongoAI.manager.SendData(className);
    }

    private void SaveAIData() {
        MongoAI.manager.CacheData(this, geneticProperties, className, AIHeuristic());
    }

    private float AIHeuristic() {
        // Our heuristic will be how far the 2 colors are as points in a 3D plane
        // Determining a heuristic (the fun part!) is for you
        // We take the negative of distance because lower distance is better

        float r = spriteRenderer.color.r;
        float g = spriteRenderer.color.g;
        float b = spriteRenderer.color.b;

        Vector3 current = new Vector3(r, g, b);
        Vector3 desired = new Vector3(desiredColor.r, desiredColor.g, desiredColor.b);
        
        float distance = -Vector3.Distance(current, desired);

        // If it is within 90px, then good estimation
        if (Mathf.Abs(distance) < 80.0f/255.0f) {
            reachGoal = true;
            transform.localScale = new Vector3(60,60,0);
        }

        return distance;
    }
}
