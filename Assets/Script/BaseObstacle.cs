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
        float distanceToPlayer = Vector3.Distance(transform.position, JellyController.Instance.transform.position);
        Vector3 directionToPlayer = JellyController.Instance.transform.position - transform.position;
        Vector3 movementDirection = -LevelManager.Instance.currentDirection; 

        bool isBehind = Vector3.Dot(directionToPlayer, movementDirection) < 0;

        if (isBehind && distanceToPlayer > Mathf.Abs(destroyZPosition))
        {
            OnDespawn();
            LevelManager.Instance.ReturnToPool(gameObject, poolIndex);
        }
    }
    protected virtual void OnDespawn() { }
}