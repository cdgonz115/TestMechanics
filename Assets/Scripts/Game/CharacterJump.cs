using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Character
{
    protected float _justJumpedCooldown;

    public Vector3 jumpStartPosition;
    [System.Serializable]
    public class JumpMechanic : PhysicsMechanic
    {
        public float jumpStrength = 6.5f;
        public float jumpStregthDecreaser = .05f;
        public float jumpInAirControl = .1f;
        public float jumpingInitialGravity = -.3f;

        public float justJumpedCooldown = .1f;
        public float durationOfJump = 2f;

        //.365,.52

        [Range(0,1)]
        public float jumpParabolaDistanceToHeightOffset;

        public int inAirJumps = 0;
    }
    Vector3 jumpTargetPosition;

    protected virtual void SetJumpTargetPosition(Vector3 position)
    {
        if (position == null) jumpTargetPosition = Vector3.negativeInfinity;
        jumpTargetPosition = position;
        //print(jumpTargetPosition);
    }
    public float Remap(float from, float fromMin, float fromMax, float toMin, float toMax)
    {
        var fromAbs = from - fromMin;
        var fromMaxAbs = fromMax - fromMin;

        var normal = fromAbs / fromMaxAbs;

        //print(normal);

        var toMaxAbs = toMax - toMin;
        var toAbs = toMaxAbs * normal;

        //print(toAbs);

        var to = toAbs + toMin;

        return to;
    }
    void DrawPath()
    {
        LaunchData launchData = gravityMechanic.ProjectileLaunch(jumpStartPosition,
            jumpTargetPosition + groundCheckMechanic.groundCheckDistance * -gravityDirection, gravityDirection);
        Vector3 previousDrawPoint = jumpStartPosition;

        int resolution = 30;
        for (int i = 1; i <= resolution; i++)
        {
            float simulationTime = i / (float)resolution * launchData.timeToTarget;
            Vector3 displacement = launchData.initialVelocity * simulationTime + Vector3.up * gravityMechanic.aproximatedConstantAcceleration * simulationTime * simulationTime / 2f;
            Vector3 drawPoint = jumpStartPosition + displacement;
            Debug.DrawLine(previousDrawPoint, drawPoint, Color.green);
            previousDrawPoint = drawPoint;
        }
    }
    public virtual void Jump(Vector3 targetPosition)
    {
        gravityMechanic.CalculateVelocityEquationValues();
        //gravityMechanic.CalculateVelocityAtFixedTime(1.14f);

        groundCheck = false;
        Vector3 direction = targetPosition - (transform.position + groundCheckMechanic.groundCheckDistance * gravityDirection);

        float angle = Vector3.Angle(-gravityDirection, direction);

        //print("Angle "+angle);

        Vector3 dirX = Vector3.ProjectOnPlane(direction, -gravityDirection);
        Vector3 dirY = direction - dirX;

        float peak = dirY.magnitude + groundCheckMechanic.groundCheckDistance;

        //print("dirY " + dirY.magnitude);
        //print("dirX " + dirX.magnitude);

        //print(2.8f + groundCheckMechanic.groundCheckDistance);
        if (dirX.magnitude < 2.8f + groundCheckMechanic.groundCheckDistance) return;


        float val1 = 1 - .99f;
        float val2 = 1 - .065f;

        float percentage;
        if (dirY.magnitude - dirX.magnitude > 0)
        {
            percentage = dirX.magnitude / dirY.magnitude;
            jumpMechanic.jumpParabolaDistanceToHeightOffset = Remap(1 - percentage, val1, val2, .35f, .9999f);
        }
        else
        {
            percentage = dirY.magnitude / dirX.magnitude;
            //print("here " + (1f - percentage));
            jumpMechanic.jumpParabolaDistanceToHeightOffset = Remap(1 - percentage, val1, val2, .35f, .52f);
        }

        //print("percentage "+ percentage);

        //.06568815 maps to 0.9999

        //.99 maps to 0.35

        //float difference = Mathf.Abs(dirX.magnitude - dirY.magnitude);

        //jumpMechanic.jumpParabolaDistanceToHeightOffset = Mathf.Clamp(difference, .37f, .52f);

        //dirX = dirX * (1 - jumpMechanic.jumpParabolaDistanceToHeightOffset);

        //dirY = dirY * (1 + jumpMechanic.jumpParabolaDistanceToHeightOffset);

        //print(dirX.magnitude);

        //float localTime = jumpMechanic.durationOfJump* (1f / Time.fixedDeltaTime);

        //print(localTime);

        //float accelerationAtEnd = -0.27736f * localTime + gravityMechanic.initialGravity;

        float avgAcceleration = gravityMechanic.CalculateLinearAcceleration(jumpMechanic.durationOfJump);

        //print("average Acceleration: " + avgAcceleration);

        //float avgAcceleration = gravityMechanic.initialGravity * Mathf.Pow(Mathf.Pow(gravityMechanic.gravityRate,50),jumpMechanic.durationOfJump);

        //print("Acceleration " + avgAcceleration);

        //float topHalf = avgAcceleration * dirX.magnitude;

        //float bottomHalf = (dirY.magnitude - (Mathf.Cos(angle) / Mathf.Sin(angle))*dirX.magnitude);

        //float experimental = Mathf.Sqrt(Mathf.Abs(topHalf / bottomHalf));

        //print("Experimental " + experimental);

        //print(dirY.magnitude);

        float initialYVelocity = avgAcceleration * jumpMechanic.durationOfJump * jumpMechanic.durationOfJump * .5f;

        //print(part1);

        float part2 = dirY.magnitude + initialYVelocity;

        //print(part2);

        float velocityY = Mathf.Sqrt(-avgAcceleration * peak * 2);

        float timeUp = Mathf.Sqrt((2 * peak) / -avgAcceleration);

        float peakPointDelta = (dirY.magnitude - peak);

        //print(peakPointDelta);

        float timeDown = Mathf.Sqrt((2 * peakPointDelta) / avgAcceleration);

        //float time = (-velocityY - gravityMechanic.initialGravity) / avgAcceleration;

        float totalTime = timeUp + timeDown;

        float velocityX = dirX.magnitude / totalTime;

        //print("time Up" + timeUp);

        //print("time Down" + timeDown);

        //float vx = dirX.magnitude * time * 2;
        //print("vx " + vx);
        //float velocityY =  Mathf.Sqrt(gravityMechanic.initialGravity * gravityMechanic.initialGravity + (dirY.magnitude * avgAcceleration *2f));
        //float velocityY =  Mathf.Sqrt(Mathf.Abs(gravityMechanic.initialGravity * gravityMechanic.initialGravity + (dirY.magnitude * avgAcceleration * 2f)));
        //float velocityY = -.55f - -27.72f * 2;

        //print("velocityUp " + velocityY);

        //in fixed update
        //velocity eq = 0 - 0.55 x - 0.0027736 Power[x,2]
        //acceleration eq =  -0.0055472x-0.55

        //in real time
        //acceleration eq = -0.27736x – 0.55
        //velocity eq = -.13868x^2 - .55x


        //-.55 * 57 = 31.35
        //+
        //(0.8661904 - .55) * 57 * .5 = 9.0114264
        //= 40.3614264
        // -.55
        //= 39.8114264
        // /57
        //=-0.7080952 || -.69844607719298245614035087719298x - .55


        //float velocityLost = -gravityMechanic.CalculateVelocityAtFixedTime(jumpMechanic.durationOfJump);

        //print("velocityLost " + velocityLost);

        //float velocityNeeded = 

        Vector3 final = -gravityDirection.normalized * (velocityY) + dirX.normalized * velocityX;

        //print("Final" + final);

        Vector3 result = gravityMechanic.ProjectileLaunch(transform.position,
            targetPosition + groundCheckMechanic.groundCheckDistance * -gravityDirection, gravityDirection).initialVelocity;

        //print("Result " + result);
        rb.velocity = result;
        _justJumpedCooldown = jumpMechanic.justJumpedCooldown;
        _timer = timerDuration;
    }
}
