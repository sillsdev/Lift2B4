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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using ICSharpCode.SharpZipLib.Zip;

namespace Lift2B4
{
    public class LiftConvert
    {
        private static string _fileName;
        private static string _listFullName;
        private static readonly XmlDocument LiftDoc = new XmlDocument();
        private static readonly XmlDocument ListDoc = new XmlDocument();
        private static readonly XmlDocument ListRange = new XmlDocument();
        private static readonly XslCompiledTransform Lift2B4X = new XslCompiledTransform();
        private static string _unitName;
        private static int _unit;
        private static int _lesson;
        //private const string BykiSchema = "byki_schema.xsd";
        private static string _folder;
        private const int MinLessonCards = 4;
        private const int IdealLessonCards = 10;
        private const int MaxLessonCards = 20;

        public LiftConvert(string fileName)
        {
            _fileName = fileName;
            var xsltSettings = new XsltSettings() { EnableDocumentFunction = true };
// ReSharper disable AssignNullToNotNullAttribute
            Lift2B4X.Load(XmlReader.Create(Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "Lift2B4.Lift2B4x.xsl")), xsltSettings, null);
// ReSharper restore AssignNullToNotNullAttribute
            LiftDoc.Load(fileName);
            ListRange.Load(fileName.Replace(".lift", ".lift-ranges"));
            _unit = 0;
            _lesson = 0;
        }

        private readonly Dictionary<string, List<string>> _lessons = new Dictionary<string, List<string>>();
        private readonly Dictionary<string, List<string>> _units = new Dictionary<string, List<string>>();
        private readonly ArrayList _unitNames = new ArrayList();
        public ArrayList Units()
        {
            _lessons.Clear();
            _usedNames.Clear();
            _units.Clear();
            _unitNames.Clear();
            var semanticDomains = LiftDoc.SelectNodes(@"//entry/citation//@lang[contains(.,'-audio')]/parent::*/parent::*/parent::*//trait[contains(@name,'semantic-domain')]/@value");
            Debug.Assert(semanticDomains != null, "no semantic domains");
            var domainList = new Dictionary<string,int>();
            foreach (XmlAttribute semanticDomain in semanticDomains)
            {
                var val = semanticDomain.Value;
                if (domainList.ContainsKey(val))
                {
                    domainList[val] += 1;
                }
                else
                {
                    domainList[val] = 1;
                }
            }
            var domains = domainList.Keys.ToList();
            domains.Sort();
            int curCardCount = 0;
            var curDomains = new List<string>();
            var baseDomain = domains.First().Substring(0,1);
            foreach (var domain in domains)
            {
                var curBase = domain.Substring(0, 1);
                var reducesSame = false;
                if (curDomains.Count > 1)
                {
                    var sameCount = HowManySame(curDomains);
                    reducesSame = curDomains.First().Substring(0, sameCount) != domain.Substring(0, sameCount);
                }
                if (curBase != baseDomain || curCardCount > MinLessonCards && (curCardCount + domainList[domain] > MaxLessonCards || curCardCount > IdealLessonCards || reducesSame))
                {
                    if (curCardCount >= MinLessonCards)
                    {
                        var lessonName = GetLessonName(curDomains);
                        _lessons[lessonName] = curDomains.ToList();
                        AddLessonToUnit(curDomains.First().Substring(0, 1), lessonName);
                    }
                    curDomains.Clear();
                    curCardCount = 0;
                }
                curCardCount += domainList[domain];
                curDomains.Add(domain);
                baseDomain = domain.Substring(0, 1);
            }
            return _unitNames;
        }

        private Dictionary<string, int> _usedNames = new Dictionary<string, int>();
        private string GetLessonName(IReadOnlyList<string> curDomains)
        {
            var first = curDomains.First();
            if (curDomains.Count == 1)
            {
                var index = first.IndexOf(' ');
                var name = first.Substring(index + 1);
                _usedNames[name] = 1;
                return name;
            }
            var sameCount = HowManySame(curDomains);
            var node = ListRange.SelectSingleNode(string.Format("//range[starts-with(@id,'semantic-domain')]/range-element[starts-with(@id,'{0} ')]/@id", first.Substring(0, sameCount)));
            Debug.Assert(node != null, "No domain node found for unit name.");
            var unitName = node.Value.Substring(sameCount + 1);
            if (_usedNames.ContainsKey(unitName))
            {
                _usedNames[unitName] += 1;
                unitName += string.Format(" {0}", _usedNames[unitName]);
            }
            else
            {
                _usedNames[unitName] = 1;
            }
            return unitName;
        }

        private void AddLessonToUnit(string unitNumber, string lesson)
        {
            var node = ListRange.SelectSingleNode(string.Format("//range[starts-with(@id,'semantic-domain')]/range-element[starts-with(@id,'{0} ')]/@id", unitNumber));
            Debug.Assert(node != null, "Missing unit name");
            var unitName = node.Value.Substring(2);
            if (_units.ContainsKey(unitName))
            {
                _units[unitName].Add(lesson);
            }
            else
            {
                _units[unitName] = new List<string>{lesson};
                _unitNames.Add(unitName);
            }
        }

        private static int HowManySame(IReadOnlyList<string> curDomains)
        {
            if (curDomains.Count <= 1) return 0;
            var first = curDomains.First();
            var sameCount = 0;
            var sameCh = true;
            while (true)
            {
                var curCh = first[sameCount];
                if (curCh == ' ') return sameCount;
                for (int i = 1; i < curDomains.Count; i++)
                {
                    if (curDomains[i][sameCount] != curCh)
                    {
                        sameCh = false;
                        break;
                    }
                }
                if (!sameCh) break;
                sameCount += 1;
            }
            return sameCount - 1;
        }

        public IList<string> Lessons(string unit)
        {
            return _units[unit];
        }

        public string Slide1Lang()
        {
            var lang = LiftDoc.SelectSingleNode("//entry[1]//definition[1]//form[1]/@lang");
            Debug.Assert(lang != null, "lang1 != null");
            return lang.Value != "en"? lang.Value: "eng";
        }

        public string Slide2Lang()
        {
            var lang = LiftDoc.SelectSingleNode("//entry[1]//citation[1]/form[1]/@lang");
            Debug.Assert(lang != null, "lang2 != null");
            return lang.Value;
        }

        public string LangFolder()
        {
            return string.Format(@"{0}xx_{1}us", Slide2Lang().ToUpper(), Slide1Lang().ToUpper());
        }

        public void SetUnit(string unit)
        {
            _unitName = unit;
            _unit += 1;
            _lesson = 0;
        }

        public void Convert(string lesson, string langFolder, string dateStamp)
        {
            var lessonCategories = WriteCategoryFile(lesson);
            _lesson += 1;
            var listSpec = string.Format(@"{0}{1:000}_{2:000}", langFolder, _unit, _lesson);
            _folder = Path.Combine(langFolder, listSpec);
            if (!Directory.Exists(_folder))
            {
                Directory.CreateDirectory(_folder);
            }
            _listFullName = Path.Combine(_folder, "list.xml");
            var sw = new StreamWriter(_listFullName, false, new UTF8Encoding(true));

            var xsltArgs = new XsltArgumentList();
            xsltArgs.AddParam("UnitTitle", "", _unitName);
            xsltArgs.AddParam("Category","",lesson);
            xsltArgs.AddParam("catFile", "", lessonCategories.AbsoluteUri);
            xsltArgs.AddParam("Unit","", string.Format("{0:00}", _unit));
            xsltArgs.AddParam("Lesson", "", string.Format("{0:00}", _lesson));
            xsltArgs.AddParam("Date", "", dateStamp);
            xsltArgs.AddParam("uuid", "", Guid.NewGuid().ToString());
            xsltArgs.AddParam("WritingSystemsFolder","",string.Format("{0}/WritingSystems/", Path.GetDirectoryName(_fileName)));

            Lift2B4X.Transform(_fileName, xsltArgs, sw);
            sw.Close();
        }

        string _tempFile = Path.GetTempFileName();
        private Uri WriteCategoryFile(string lesson)
        {
            var fs = new FileStream(_tempFile, FileMode.Create);
            var xw = new XmlTextWriter(fs, Encoding.UTF8);
            var catDoc = new XmlDocument();
            catDoc.LoadXml("<root/>");
            foreach (var category in _lessons[lesson])
            {
                var catElem = catDoc.CreateElement("category");
                catElem.InnerText = category;
                Debug.Assert(catDoc.DocumentElement != null, "catDoc.DocumentElement != null");
                catDoc.DocumentElement.AppendChild(catElem);
            }
            catDoc.WriteTo(xw);
            catDoc.RemoveAll();
            xw.Close();
            fs.Close();
            return new Uri("file:///" + _tempFile);
        }

        //public void CopySchema()
        //{
        //    const bool overwrite = true;
        //    File.Copy(BykiSchema, Path.Combine(_folder, BykiSchema), overwrite);
        //}

        public void CopyAudio()
        {
            ListDoc.Load(_listFullName);
            var nsmgr = new XmlNamespaceManager(LiftDoc.NameTable);
            nsmgr.AddNamespace("b4x", "http://www.transparent.com/xml/BykiList/v1-transitional");
            var audioNodes = ListDoc.SelectNodes("//b4x:side1_sound/@url | //b4x:side2_sound/@url", nsmgr);
            var srcBaseFolder = Path.GetDirectoryName(_fileName);
            Debug.Assert(srcBaseFolder != null, "Need path on input name for audio files");
            var srcFolder = Path.Combine(srcBaseFolder, "audio");
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
                    File.Copy(Path.Combine(srcFolder, audioName), Path.Combine(dstFolder, audioName), overwrite);
                    //var audioNameWoExt = Path.GetFileNameWithoutExtension(audioName);
                    //var pcmTemp = Path.Combine(Path.GetTempPath(),Path.GetFileNameWithoutExtension(Path.GetTempFileName()) + ".wav");
                    //var p1 = new Process();
                    //p1.StartInfo.FileName = "lame.exe";
                    //p1.StartInfo.Arguments = string.Format("--decode {0} {1}", Path.Combine(srcFolder, audioName), pcmTemp);
                    //p1.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    //p1.Start();
                    //p1.WaitForExit();
                    //var p2 = new Process();
                    //p2.StartInfo.FileName = "oggenc2.exe";
                    //p2.StartInfo.Arguments = string.Format("--output={0} {1}", Path.Combine(dstFolder, audioNameWoExt + ".ogg"), pcmTemp);
                    //p2.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    //p2.Start();
                    //p2.WaitForExit();
                    //File.Delete(pcmTemp);
                    //audioNode.InnerText = audioNode.InnerText.Replace(".mp3", ".ogg");
                }
            }
            ListDoc.Save(_listFullName);
            ListDoc.RemoveAll();
        }

        public void Package(bool deleteFolder)
        {
            var folder = Path.GetDirectoryName(_listFullName);
            var z = new FastZip();
            const bool recurse = true;
            z.CreateZip(folder + ".b4x", folder, recurse, null);
            Debug.Assert(folder != null, "Folder needed on output for temporary files");
            if (deleteFolder)
            {
                Directory.Delete(folder, recurse);
            }
        }
    }
}
