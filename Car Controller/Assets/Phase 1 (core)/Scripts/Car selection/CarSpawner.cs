using UnityEngine;

public class CarSpawner : MonoBehaviour
{
    [Header("Car Prefabs Array")]
    public GameObject[] carPrefabs; // Ensure this is explicitly public!

    [Header("Spawn Settings")]
    public Transform spawnPoint; // Ensure this is explicitly public!

    void Awake()
    {
        SettingsManager.LoadSettings();
        SpawnSelectedCar();
    }

    void SpawnSelectedCar()
    {
        int index = SettingsManager.SelectedCarIndex;
        if (index < 0 || index >= carPrefabs.Length) return;

        // Destroy any existing car
        GameObject existingCar = GameObject.FindGameObjectWithTag("Player");
        if (existingCar != null) Destroy(existingCar);

        // Spawn the new car
        GameObject newCar = Instantiate(carPrefabs[index], spawnPoint.position, spawnPoint.rotation);
        newCar.tag = "Player";

        // --- LINK THE CAMERA TO THE NEW CAR ---
        FreeLookCamera cameraScript = FindObjectOfType<FreeLookCamera>();
        if (cameraScript != null) cameraScript.SetCar(newCar);
        // ---------------------------------------
        
        // --- FIND THE UI AND LINK IT TO THE NEW CAR ---
        // 1. Find the Canvas UIManager
        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null) uiManager.SetCurrentCar(newCar);

        // 2. Find and update the HUD
        VehicleHUD hud = FindObjectOfType<VehicleHUD>();
        if (hud != null) hud.SetCar(newCar);
        
        // 3. Ensure the MobileCarInput and VehicleInputHandler also refresh their links 
        // (They will get this directly from the UIManager, but we can keep this just in case)
        MobileCarInput mobile = newCar.GetComponent<MobileCarInput>();
        if(mobile != null) mobile.SetCar(newCar);
        // ---------------------------------------------
        
        Debug.Log($"Spawned car: {newCar.name}");
    }
}
