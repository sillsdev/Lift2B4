// ---------------------------------------------------------------------------------------------
#region // Copyright (c) 2015, SIL International. All Rights Reserved.
// <copyright from='2015' to='2015' company='SIL International'>
//		Copyright (c) 2015, SIL International. All Rights Reserved.
//
//		Distributable under the terms of either the Common Public License or the
//		GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright>
#endregion
//
// File: LiftConvert.cs
// Responsibility: Greg Trihus
// ---------------------------------------------------------------------------------------------
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Xsl;

namespace Lift2B4
{
    public class LiftConvert
    {
        private static string _fileName;
        private static string _listFullName;
        private static readonly XmlDocument LiftDoc = new XmlDocument();
        private static readonly XmlDocument ListDoc = new XmlDocument();
        private static readonly XslCompiledTransform Lift2B4X = new XslCompiledTransform();
        private static int _lesson;
        private const string BykiSchema = "byki_schema.xsd";
        private static string _folder;

        public LiftConvert(string fileName)
        {
            _fileName = fileName;
            var xsltSettings = new XsltSettings() { EnableDocumentFunction = true };
// ReSharper disable AssignNullToNotNullAttribute
            Lift2B4X.Load(XmlReader.Create(Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "Lift2B4.Lift2B4x.xsl")), xsltSettings, null);
// ReSharper restore AssignNullToNotNullAttribute
            LiftDoc.Load(fileName);
            _lesson = 0;
        }

        private readonly ArrayList _categories = new ArrayList();
        public ArrayList Categories()
        {
            var catNodes = LiftDoc.SelectNodes("//field[@type='Semantic Field']//text[following::text = . and not(preceding::text = .)]");
            Debug.Assert(catNodes != null, "catNodes != null");
            foreach (XmlNode catNode in catNodes)
            {
                var catText = catNode.InnerText.Trim();
                if (!_categories.Contains(catText))
                {
                    _categories.Add(catText);
                }
            }
            return _categories;
        }

        public string Slide1Lang()
        {
            var lang = LiftDoc.SelectSingleNode("//entry[1]//definition[1]//form[1]/@lang");
            Debug.Assert(lang != null, "lang1 != null");
            return lang.Value;
        }

        public string Slide2Lang()
        {
            var lang = LiftDoc.SelectSingleNode("//entry[1]//citation[1]/form[1]/@lang");
            Debug.Assert(lang != null, "lang2 != null");
            return lang.Value;
        }

        public void Convert(string cat, string lang1, string lang2, string dateStamp)
        {
            _lesson += 1;
            var listSpec = string.Format(@"{0}xx_{1}us001_{2:000}", lang2, lang1, _lesson);
            _folder = Path.Combine(string.Format(@"{0}xx_{1}us001", lang2, lang1), listSpec);
            if (!Directory.Exists(_folder))
            {
                Directory.CreateDirectory(_folder);
            }
            _listFullName = Path.Combine(_folder, listSpec + ".xml");
            var sw = new StreamWriter(_listFullName, false, new UTF8Encoding(true));

            var xsltArgs = new XsltArgumentList();
            xsltArgs.AddParam("Category","",cat);
            xsltArgs.AddParam("Lesson", "", string.Format("{0:00}", _lesson));
            xsltArgs.AddParam("Date", "", dateStamp);
            xsltArgs.AddParam("WritingSystemsFolder","",string.Format("{0}/WritingSystems/", Path.GetDirectoryName(_fileName)));

            Lift2B4X.Transform(_fileName, xsltArgs, sw);
            sw.Close();
        }

        public void CopySchema()
        {
            const bool overwrite = true;
            File.Copy(BykiSchema, Path.Combine(_folder, BykiSchema), overwrite);
        }

        public void CopyAudio()
        {
            ListDoc.Load(_listFullName);
            var audioNodes = ListDoc.SelectNodes("//side1_sound/@url | //side2_sound/@url");
            var srcFolder = Path.Combine(Path.GetDirectoryName(_fileName), "audio");
            if (audioNodes != null)
            {
                var dstFolder = Path.Combine(_folder, "sounds");
                if (!Directory.Exists(dstFolder))
                {
                    Directory.CreateDirectory(dstFolder);
                }
                foreach (XmlAttribute audioNode in audioNodes)
                {
                    var audioName = Path.GetFileName(audioNode.InnerText);
                    const bool overwrite = true;
                    File.Copy(Path.Combine(srcFolder, audioName), Path.Combine(_folder, audioNode.InnerText), overwrite);
                }
            }
            ListDoc.RemoveAll();
        }
    }
}
