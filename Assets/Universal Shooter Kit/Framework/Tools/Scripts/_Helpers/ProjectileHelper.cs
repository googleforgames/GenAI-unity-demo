using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class of static functions to help with ballistic projectile calculations.
/// These functions assume that gravity acts in the y-axis.
/// </summary>
public class ProjectileHelper
{
    /// <summary>
    /// Given a maximum range (on flat ground) and gravity, what is the maximum speed the projectile can be launched?
    /// Use case: The player can only throw grenades with a max range of 30 meters on flat ground. What is the max speed he can throw it? </summary>
    /// <param name="range"> Maximum range of the projectile on flat ground, in meters</param>
    /// <param name="gravity_negative"> Acceleration due to gravity on y-axis. Should be negative, eg -9.8f </param>
    /// <param name="timeToLand"> Outputs the time taken for the projectile to reach max range on flat ground. In seconds. </param>
    /// <returns> Returns the speed needed to reach the max range.</returns>
    public static float ComputeSpeedToReachMaxFlatRange(float range, float gravity_negative, out float timeToLand)
    {
        // to reach max range, we need to throw it at 45 degrees
        const float cos45deg = 0.70710678118f;
        timeToLand           = Mathf.Sqrt(range / (-0.5f * gravity_negative));
        float forwardSpeed   = range / timeToLand;
        float speed          = forwardSpeed / cos45deg;
        
        return speed;
    }

    /// <summary>
    /// Given a start position, gravity and initial speed, can a projectile reach a target position?
    /// Use case: If my cannonballs always fire at 200m/sec, can it reach a target position?
    /// </summary>
    /// <param name="startPosition"> Starting Position of the projectile. </param>
    /// <param name="targetPosition"> MatchTarget Position of the projectile. </param>
    /// <param name="gravity_negative"> Acceleration due to gravity on y-axis. Should be negative, eg -9.8f </param>
    /// <param name="speed"> Initial speed of the projectile. </param>
    /// <returns> Returns true if the target position can be reached with the given parameters. </returns>
    public static bool CanReachTargetWithSpeed(
        Vector3 startPosition,
        Vector3 targetPosition,
        float gravity_negative,
        float speed)
    {
        Vector3 startToEndFlat = targetPosition - startPosition;
        startToEndFlat.y       = 0.0f;
        float flatDistance     = startToEndFlat.magnitude;
        float heightDiff       = targetPosition.y - startPosition.y;
        
        float toRoot           = Mathf.Pow(speed, 4.0f);
        toRoot                 += gravity_negative * (-gravity_negative * flatDistance * flatDistance + 2.0f * heightDiff * speed * speed);
        if (toRoot < 0.0f)
        {
            return false;
        }
        
        return true;
    }

    /// <summary>
    /// Given a start position, gravity and initial speed, what direction should we fire the projectile to hit a target position?
    /// </summary>
    /// <param name="startPosition"> Starting Position of the projectile. </param>
    /// <param name="targetPosition"> MatchTarget Position of the projectile. </param>
    /// <param name="gravity_negative"> Acceleration due to gravity on y-axis. Should be negative, eg -9.8f </param>
    /// <param name="speed"> Initial speed of the projectile. </param>
    /// <param name="direction1"> First possible direction for the projectile </param>
    /// <param name="direction2"> Second possible direction for the projectile </param>
    /// <returns> Returns true if the target position can be reached with the given parameters. </returns>
    public static bool ComputeDirectionToHitTargetWithSpeed(
        Vector3 startPosition,
        Vector3 targetPosition,
        float gravity_negative,
        float speed,
        out Vector3 direction1,
        out Vector3 direction2)
    {
        bool canReach            = true;
        Vector3 startToEndFlat = targetPosition - startPosition;
        startToEndFlat.y       = 0.0f;
        float flatDistance     = startToEndFlat.magnitude;
        float heightDiff       = targetPosition.y - startPosition.y;
        
        float toRoot           = Mathf.Pow(speed, 4.0f);
        toRoot                 += gravity_negative * (-gravity_negative * flatDistance * flatDistance + 2.0f * heightDiff * speed * speed);
        if (toRoot < 0.0f)
        {
            // we can't reach the target
            toRoot   = 0.0f;
            canReach = false;
        }
        
        float root                 = Mathf.Sqrt(toRoot);
        Vector3 startToEndFlatNorm = startToEndFlat.normalized;
        Vector4 horizonAxis        = Vector3.Cross(startToEndFlatNorm, Vector3.up);
        
        float angle1    = ((speed * speed) + root) / (-gravity_negative * flatDistance);
        angle1          = Mathf.Atan(angle1);
        float angle1Deg = Mathf.Rad2Deg * angle1;
        direction1      = Quaternion.AngleAxis(angle1Deg, horizonAxis) * startToEndFlatNorm;    
        
        float angle2    = ((speed * speed) - root) / (-gravity_negative * flatDistance);
        angle2          = Mathf.Atan(angle2);
        float angle2Deg = Mathf.Rad2Deg * angle2;
        direction2      = Quaternion.AngleAxis(angle2Deg, horizonAxis) * startToEndFlatNorm;
        
        return canReach;    
    }

    /// <summary>
    /// Given a start position, initial velocity (with direction) and gravity, how much time 
    /// is needed to reach the ground (at a specified height level).
    /// Use case: Artillery has begun firing upon your position. How much time do you have to evacuate / take cover?
    /// </summary>
    /// <param name="startPosition"> Starting position of the projectile. </param>
    /// <param name="velocity"> Initial velocity of the projectile. </param>
    /// <param name="groundLevel"> The y-value of the ground level. </param>
    /// <param name="gravity_negative"> Acceleration due to gravity on y-axis. Should be negative, eg -9.8f </param>
    /// <param name="timeToHit"> Output: Time to reach the groundlevel specified. </param>
    /// <returns> Returns true if the ground level can be reached with the given parameters. </returns>
    public static bool ComputeTimeToHitGround(
        Vector3 startPosition,
        Vector3 velocity,
        float groundLevel,
        float gravity_negative,
        out float timeToHit)
    {
        float heightDiff = groundLevel - startPosition.y;
        float speed      = velocity.y;
        float b2minus4ac = (speed * speed) + (2.0f * gravity_negative * heightDiff);
        if (b2minus4ac < 0.0f)
        {
            timeToHit = -1.0f;
            return false;
        }
        
        float sqrtB2minus4ac = Mathf.Sqrt(b2minus4ac);
        float time1          = (-speed + sqrtB2minus4ac) / gravity_negative;
        float time2          = (-speed - sqrtB2minus4ac) / gravity_negative;
        
        // two possible times to hit, since the projectile goes up and down
        // take the bigger one since we assume we want to hit the ground when going down
        float timeNeeded = Mathf.Max(time1, time2);
        if (timeNeeded < 0.0f)
        {
            timeToHit = -1.0f;
            return false;
        }
        
        timeToHit = timeNeeded;
        return true;
    }

    /// <summary>
    /// Determine what position a projectile will be after a certain time, given its current position and velocity.
    /// Use case: To display the trajectory of a grenade before you throw it
    /// </summary>
    /// <param name="currentPosition"> Current position of the projectile. </param>
    /// <param name="velocity"> Current velocity of the projectile. </param>
    /// <param name="gravity_negative"> Acceleration due to gravity on y-axis. Should be negative, eg -9.8f </param>
    /// <param name="timeAhead"> The amount of time in the future at which you want to know the position</param>
    /// <returns> Returns the position of the projectile after the specified time. </returns>
    public static Vector3 ComputePositionAtTimeAhead(Vector3 currentPosition, Vector3 velocity, float gravity_negative, float timeAhead)
    {
        Vector3 acceleration = new Vector3(0.0f, gravity_negative, 0.0f);
        Vector3 move = (velocity * timeAhead) + (0.5f * acceleration * timeAhead * timeAhead);
        return currentPosition + move;
    }

    /// <summary>
    /// Determine the velocity of a projectile after a certain time, given its current position and velocity.
    /// </summary>
    /// <param name="currentPosition"> Current position of the projectile. </param>
    /// <param name="currentVelocity"> Current velocity of the projectile. </param>
    /// <param name="gravity_negative"> Acceleration due to gravity on y-axis. Should be negative, eg -9.8f </param>
    /// <param name="timeAhead"> The amount of time in the future at which you want to know the velocity</param>
    /// <returns> Returns the velocity of the projectile after the specified time. </returns>
    public static Vector3 ComputeVelocityAtTimeAhead(
        Vector3 currentPosition,
        Vector3 currentVelocity,
        float gravity_negative,
        float timeAhead)
    {
        Vector3 acceleration = new Vector3(0.0f, gravity_negative, 0.0f);
        return currentVelocity + (acceleration * timeAhead);
    }

    /// <summary>
    /// Determine the velocity needed to hit a target position in exactly the given amount of time.
    /// Use case: you want to fire a catapult projectile to land at a specific position, giving the player exactly 4 seconds of warning time to escape
    /// </summary>
    /// <param name="startPosition"> Starting position of the projectile. </param>
    /// <param name="targetPosition"> MatchTarget position of the projectile. </param>
    /// <param name="gravity_negative"> Acceleration due to gravity on y-axis. Should be negative, eg -9.8f </param>
    /// <param name="timeToTargetPosition"> The amount of time the projectile should take to reach the target</param>
    /// <returns> Returns the necessary initial velocity of the projectile. </returns>
    public static Vector3 ComputeVelocityToHitTargetAtTime(
        Vector3 startPosition, 
        Vector3 targetPosition,
        float gravity_negative,
        float timeToTargetPosition)
    {
        if (timeToTargetPosition <= 0.0f)
        {
            Debug.LogError("ComputeStartVelocity called with invalid timeToTargetPosition");
        }
    
        // calculate forward speed
        Vector3 startToEndFlat = targetPosition - startPosition;
        startToEndFlat.y       = 0.0f;
        float flatDistance     = startToEndFlat.magnitude;
        float forwardSpeed     = flatDistance / timeToTargetPosition;
        
        // calculate vertical speed
        float heightDiff       = targetPosition.y - startPosition.y;
        float upSpeed          = (heightDiff - (0.5f * gravity_negative * timeToTargetPosition * timeToTargetPosition)) / timeToTargetPosition;
        
        // initialize velocity
        Vector3 velocity       = startToEndFlat.normalized * forwardSpeed;
        velocity.y             = upSpeed;
        
        return velocity;
    }

    /// <summary>
    /// Update a projectile's current position and velocity, given the gravity and delta time
    /// </summary>
    /// <param name="currentPosition"> Current position of the projectile. Will be modified to give the new position. </param>
    /// <param name="currentVelocity"> Current velocity of the projectile. Will be modified to give the new velocity. </param>
    /// <param name="gravity_negative"> Acceleration due to gravity on y-axis. Should be negative, eg -9.8f </param>
    /// <param name="deltaTime"> The amount of time elapsed since the last update</param>
    public static void UpdateProjectile(
        ref Vector3 currentPosition,
        ref Vector3 currentVelocity,
        float gravity_negative,
        float deltaTime)
    {
        currentPosition   += currentVelocity * deltaTime;
        currentVelocity.y += gravity_negative * deltaTime;
    }

    /// <summary>
    /// Determine the gravity needed to reach a target distance and height with a given speed and elevation angle
    /// </summary>
    /// <param name="distanceOffset"> Flat distance away from the starting position. </param>
    /// <param name="heightOffset"> Height difference from the starting position. </param>
    /// <param name="elevationRadians"> Elevation angle the projectile will be fired at. </param>
    /// <param name="speed"> The initial speed of the projectile. </param>
    /// <returns> Returns the necessary gravity to reach the target. </returns>
    public static float ComputeGravityToReachTargetWithSpeedAndElevation(
        float distanceOffset, 
        float heightOffset, 
        float elevationRadians, 
        float speed)
    {
        float cosElevation = Mathf.Cos(elevationRadians);
        float result = 2.0f * (heightOffset - distanceOffset * Mathf.Tan(elevationRadians)) * speed * speed * cosElevation * cosElevation;
        result = result / (distanceOffset * distanceOffset);
        return result;
    }

    /// <summary>
    /// Determine the speed needed to reach a target position given a starting elevation.
    /// </summary>
    /// <param name="startPosition"> Starting position of the projectile. </param>
    /// <param name="targetPosition"> MatchTarget position of the projectile. </param>
    /// <param name="elevationRadians"> Elevation angle the projectile will be fired at. This should be less than PiBy2 radians, ie 90degrees. </param>
    /// <param name="gravity_negative"> Acceleration due to gravity on y-axis. Should be negative, eg -9.8f </param>
    /// <param name="outSpeed"> Output: The calculated speed needed. </param>
    /// <returns> Returns true if the target can be reached with the given elevation, otherwise false </returns>
    public static bool ComputeSpeedToReachTargetWithElevation(Vector3 startPosition, Vector3 targetPosition, float elevationRadians, float gravity_negative, ref float outSpeed)
    {
        outSpeed = 0.0f;

        Vector3 flatOffset = targetPosition - startPosition;
        flatOffset.y = 0.0f;
        float horizontalOffset = flatOffset.magnitude;
        float verticalOffset = targetPosition.y - startPosition.y;

        if (Mathf.Abs(elevationRadians - (Mathf.PI * 0.5f)) < Mathf.Epsilon)
        {
            // if angle is vertical (90deg), we cannot reach targets to the side
            if (horizontalOffset > Mathf.Epsilon)
            {
                return false;
            }

            // if the target is below me, any velocity will do, just give zero
            if (verticalOffset <= 0.0f)
            {
                return true;
            }

            // if the target is above me, any velocity higher than a minimum speed will do.
            outSpeed = Mathf.Sqrt(-2.0f * gravity_negative * verticalOffset);
            return true;
        }

        float cosAngle = Mathf.Cos(elevationRadians);
        float tanAngle = Mathf.Tan(elevationRadians);

        if (verticalOffset >= horizontalOffset * tanAngle)
        {
            // target is too high
            return false;
        }

        outSpeed = gravity_negative * horizontalOffset * horizontalOffset;
        outSpeed /= 2.0f * cosAngle * cosAngle * (verticalOffset - horizontalOffset * tanAngle);
        outSpeed = Mathf.Sqrt(outSpeed);

        // To get the actual 3D velocity:
        //      Vector3 initialVelocity = flatOffset.normalized * Mathf.Cos(angle) + Vector3.up * Mathf.Sin(angle);
        //      initialVelocity *= outSpeed;

        return true;
    }

    /// <summary>
    /// Determine the speed needed to reach a certain range (on flat ground) given a starting elevation
    /// </summary>
    /// <param name="elevationRadians"> Elevation angle the projectile will be fired at. </param>
    /// <param name="range"> Flat distance away from the starting position. </param>
    /// <param name="gravity_negative"> Acceleration due to gravity on y-axis. Should be negative, eg -9.8f </param>
    /// <param name="speed"> Output: Speed of projectile needed to reach the wanted range. </param>
    /// <returns> Returns true if the range can be reached. This could be false, for eg if the elevation is lower than zero. </returns>
    public static bool ComputeSpeedToReachFlatRangeWithElevation(
        float elevationRadians, 
        float range, 
        float gravityNegative, 
        out float speed)
    {
        if (elevationRadians <= 0.0f)
        {
            speed = 0.0f;
            return false;
        }

        float sinElevation  = Mathf.Sin(elevationRadians);
        float cosElevation  = Mathf.Cos(elevationRadians);
        float toRoot        = (-1.0f * gravityNegative * range) / (2.0f * sinElevation / cosElevation);
        if (toRoot < 0.0f)
        {
            speed = 0.0f;
            return false;
        }

        speed = Mathf.Sqrt(toRoot) / cosElevation;
        return true;
    }

    /// <summary>
    /// Given an initial speed, what elevation angle is needed to hit a target at a specified height offset and flat distance away?
    /// </summary>
    /// <param name="heightOffset"> Difference in height of the target from the starting position </param>
    /// <param name="distanceOffset"> Flat distance away from the starting position. </param>
    /// <param name="gravity_negative"> Acceleration due to gravity on y-axis. Should be negative, eg -9.8f </param>
    /// <param name="speed"> Initial speed of the projectile. </param>
    /// <param name="useSmallerAngle"> Whether to calculate the smaller or larger elevation angle, since two elevations are possible. </param>
    /// <param name="angleRadians"> Output: the elevation angle needed. </param>
    /// <returns> Returns true if the target can be reached. </returns>
    public static bool ComputeElevationToHitTargetWithSpeed(
        float heightOffset, 
        float distanceOffset, 
        float gravityNegative, 
        float speed, 
        bool useSmallerAngle, 
        out float angleRadians)
    {
        float speedSq              = speed * speed;
        float toRoot               = speedSq * speedSq;
        float minusGravityDistance = -gravityNegative * distanceOffset;
        toRoot += gravityNegative * (minusGravityDistance * distanceOffset + 2.0f * heightOffset * speedSq);
        if (toRoot < 0.0f)
        {
            // we can't reach the target
            angleRadians = 0.0f;
            return false;
        }

        float root          = Mathf.Sqrt(toRoot);
        float angle1Radians = (speedSq + root) / minusGravityDistance;
        float angle2Radians = (speedSq - root) / minusGravityDistance;

        if (useSmallerAngle)
        {
            angleRadians = Mathf.Min(angle1Radians, angle2Radians);
            angleRadians = Mathf.Atan(angleRadians);
            return true;
        }
        else
        {
            angleRadians = Mathf.Max(angle1Radians, angle2Radians);
            angleRadians = Mathf.Atan(angleRadians);
            return true;
        }
    }
}