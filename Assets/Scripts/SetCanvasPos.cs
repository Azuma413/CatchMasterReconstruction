using UnityEngine;

public class SetCanvasPos : MonoBehaviour
{
    [SerializeField] private GameObject canvas;
    public float offset = 0.3f;
    // Start is called before the first frame update
    void Start()
    {
        if (canvas == null)
        {
            Debug.LogError("Canvas object is not set.");
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 canvas_pos = Camera.main.transform.position;
        canvas_pos.z += offset; // カメラの位置から少し前に移動
        canvas.transform.position = canvas_pos; // カメラの位置に移動
        canvas.transform.LookAt(Camera.main.transform); // カメラの方向を向く
    }
}
