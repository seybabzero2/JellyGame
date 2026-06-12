using UnityEngine;

public abstract class BaseObstacle : MonoBehaviour
{
    public float destroyZPosition = -10f;
    [HideInInspector] public int poolIndex; 

    protected virtual void Update()
    {
        Move();
        CheckBounds();
    }

    protected virtual void Move()
    {
        float currentSpeed = LevelManager.Instance.currentSpeed;
        transform.Translate(Vector3.back * currentSpeed * Time.deltaTime);
    }

    private void CheckBounds()
    {
        if (transform.position.z < destroyZPosition)
        {
            OnDespawn();
            LevelManager.Instance.ReturnToPool(gameObject, poolIndex); 
        }
    }

    protected virtual void OnDespawn() { }
}