using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityStandardAssets._2D
{
    [RequireComponent(typeof(PlatformerCharacter2D))]
    public class Platformer2DUserControl : MonoBehaviour {

        public GameObject jumper;
        public GameObject goalText;
        private PlatformerCharacter2D movementController;
        private bool isJumping;
        
        private bool clearedCourse = false;
        private static string className;
        private static float timeScale;

        [Header("Mongo AI Properties")]
        // We need to specify what properties to save
        public List<string> geneticProperties;

        // Jump when the distance * width of the box is greater than this
        public float jumpDistanceThreshold = 1.0f;
        public float jumpForce = 650;
        public float movementSpeed = 10;

        private void Start() {
            if (!MongoAI.manager.HasInternetConnection()) {
                // If we cannot connect to MongoAI, just destroy this object.
                // This function for handling if the player is unable to connect to the remote database
                // so your game can be robust and handle if you cannot connect to MongoAI.
                // In our case we will just destroy Jumpman because the purpose of this level is
                // to test the AI
                Debug.LogError("Connection to the database failed");
                Destroy(gameObject);
            }

            className = GetType().Name;
        }
        
        // The following functions are only related to gameplay mechanics
        // They do not contain any MongoAI calls
        #region Gameplay Mechanics

        private void Awake() {
            goalText.SetActive(false);
            movementController = GetComponent<PlatformerCharacter2D>();
        }

        private void OnCollisionEnter2D(Collision2D other) {
            // If we hit an obstacle then this trial is done
            if (other.gameObject.CompareTag("Obstacle")) {
                Debug.Log("Jumpman hit the box. Resetting.");
                ResetJumpman();
            } else if (other.gameObject.CompareTag("Goal")) {
                Debug.LogWarning("Jumpman cleared the obstacle course!");
                goalText.SetActive(true);
                clearedCourse = true;
            }
        }

        private void FixedUpdate() {
            ExecuteAi();
        }

        private void ExecuteAi() {
            float movementDirection = 1; // Always move to the right
            bool crouch = false; // We don't need to crouch ever
            bool shouldJump = ShouldJump();
            isJumping = shouldJump;

            movementController.Move(movementDirection, crouch, shouldJump);
        }

        public void SetTimeScale(float scale) {
            timeScale = scale;
            Time.timeScale = scale;
        }
        
        public void TogglePause() {
            Time.timeScale = Time.timeScale.Equals(0) ? timeScale : 0;
        }
        
        #endregion


        private bool ShouldJump() {
            // The AI will determine if it should jump
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right, 50);

            if (hit.transform != null) {
                GameObject obstacle = hit.transform.gameObject;

                if (obstacle.CompareTag("Obstacle")) {
                    // This obstacle is in front of us

                    // Check the distance
                    float obstacleWidth = obstacle.GetComponent<Collider2D>().bounds.size.x;
                    float distance = Mathf.Abs(obstacle.transform.position.x - transform.position.x);
                    
                    // Wider obstacles should have longer distance before jumping
                    float distanceThreshold = jumpDistanceThreshold * obstacleWidth;

                    // If we are closer than the distance threshold then jump
                    return distance < distanceThreshold && !isJumping;
                }
            }

            return false;
        }

        private void ResetJumpman() {
            // Reset Jumpman back to the start
            GameObject newJumper = Instantiate(jumper);
            newJumper.transform.position = new Vector2(0, 2);
            newJumper.GetComponent<Platformer2DUserControl>().UpdateProperties();

            // Before destroying, save the data
            SaveAIData();
            SubmitAIData();

            // Now destroy this jumpman
            Destroy(gameObject);
        }

        private void SaveAIData() {
            MongoAI.manager.CacheData(this, geneticProperties, className, AIHeuristic());
        }

        private void SubmitAIData() {
            MongoAI.manager.SendData(className);
        }

        private float AIHeuristic() {
            // Our score is: How far have we gone?
            // This works because our platformer always moves to the right.
            // If we cleared the course automatically use 100 as the score
           
            return clearedCourse ? 100 : Mathf.Abs(transform.position.x);
        }

        public void UpdateProperties() {
            MongoAI.manager.PopulateProperties(this, geneticProperties, className, 2, 0.15f, false);
            OverwriteDefaultMovementValues();

            Debug.Log("Force: " + jumpForce);
            Debug.Log("Speed: " + movementSpeed);
            Debug.Log("Distance threshold: " + jumpDistanceThreshold);
        }

        private void OverwriteDefaultMovementValues() {
            // We need to overwrite the Movement Controller with our AI data
            movementController.m_JumpForce = jumpForce;
            movementController.m_MaxSpeed = movementSpeed;
        }

        public void ResetAllData() {
            // This will clear all AI data and refresh the simulation
            // Useful for if you want to see it work from the beginning
            MongoAI.manager.ClearData(className);
            Debug.LogWarning("Data cleared");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
