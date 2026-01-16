using TMPro;
using UnityEngine;

namespace RunFast2.Scripts.Controller
{
    public class VersionUI : MonoBehaviour
    {
        public TMP_Text versionText;

        private void Start()
        {
            versionText.text = Application.version;
        }
    }
}