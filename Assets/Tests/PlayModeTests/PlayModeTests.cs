using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayModeTests
{
    GameObject player;

    [SetUp]
    public void Setup()
    {
        player = new GameObject("Player");
        player.AddComponent<Rigidbody>();
    }

    [UnityTest]
    public IEnumerator Player_FallsWithGravity()
    {
        float startY = player.transform.position.y;
        yield return new WaitForSeconds(1f);
        float endY = player.transform.position.y;

        Assert.Less(endY, startY); // ตรวจว่า Y ลดลง
    }

    [TearDown]
    public void Cleanup()
    {
        GameObject.Destroy(player);
    }
}
