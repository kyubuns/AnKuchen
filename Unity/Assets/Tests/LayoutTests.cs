using System.Collections;
using AnKuchen.Layout;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class LayoutTests
    {
        [UnityTest]
        public IEnumerator Editし直すと前のオブジェクトは消える()
        {
            var test1Object = Resources.Load<GameObject>("Test1");
            var ui = TestUtils.Instantiate(test1Object);

            // HogeButton, FugaButton, PiyoButton
            Assert.AreEqual(3, ui.Get("Buttons").transform.childCount);
            using (var editor = Layouter.Edit(ui.GetMapper("HogeButton")))
            {
                editor.Create();
                editor.Create();
                editor.Create();
            }

            yield return null;

            // 追加で3つ作った
            Assert.AreEqual(6, ui.Get("Buttons").transform.childCount);

            using (var editor = Layouter.Edit(ui.GetMapper("HogeButton")))
            {
                editor.Create();
                editor.Create();
                editor.Create();
            }

            yield return null;

            // 3つ消えて、3つ作った
            Assert.AreEqual(6, ui.Get("Buttons").transform.childCount);
        }
    }
}
