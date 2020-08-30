using AnKuchen.UIMapper;
using UnityEngine;
using UnityEngine.UI;

namespace AnKuchen.Sample
{
    public class Sample : MonoBehaviour
    {
        [SerializeField] private UICache root = default;

        public void Start()
        {
            root.Get<Text>("HogeButton/Text").text = "Hoge";
            root.Get<Text>("FugaButton/Text").text = "Fuga";
            root.Get<Text>("PiyoButton/Text").text = "Piyo";
        }
    }
}
