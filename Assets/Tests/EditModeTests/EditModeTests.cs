using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class EditModeTests
{
     [Test]
    public void Addition_WorksCorrectly()
    {
        int a = 2;
        int b = 3;
        int result = a + b;
        Assert.AreEqual(5, result);  // ทดสอบว่าผลลัพธ์ตรงกับที่คาดไว้
    }

    [Test]
    public void String_NotNull()
    {
        string name = "Player";
        Assert.IsNotNull(name);  // ตรวจว่าค่าไม่เป็น null
    }
}
