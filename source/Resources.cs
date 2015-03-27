using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using KSP;
using UnityEngine;
using KSPPluginFramework;

namespace RealScience
{
    internal class Resources
    {
        //WHERE SHOULD THESE BE???
        internal static String PathApp = KSPUtil.ApplicationRootPath.Replace("\\", "/");
        internal static String PathRealScience = string.Format("{0}GameData/RealScience", PathApp);
        //internal static String PathPlugin = string.Format("{0}/{1}", PathTriggerTech, KSPAlternateResourcePanel._AssemblyName);
        internal static String PathPlugin = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        internal static String PathPluginResources = string.Format("{0}/Resources", PathRealScience);
        internal static String PathPluginToolbarIcons = string.Format("{0}/Resources/ToolbarIcons", PathRealScience);
        internal static String PathPluginTextures = string.Format("{0}/Resources/Textures", PathRealScience);
        //internal static String PathPluginData = string.Format("{0}/Data", PathPlugin);
        internal static String PathPluginSounds = string.Format("{0}/Resources/Sounds", PathPlugin);

        internal static String DBPathTestFlight = string.Format("RealScience");
        internal static String DBPathPlugin = string.Format("RealScience/{0}", RealScience.RealScienceManager._AssemblyName);
        internal static String DBPathResources = string.Format("{0}/Resources", DBPathPlugin);
        internal static String DBPathToolbarIcons = string.Format("{0}/Resources/ToolbarIcons", DBPathPlugin);
        internal static String DBPathTextures = string.Format("{0}/Resources/Textures", DBPathPlugin);
        internal static String DBPathPluginSounds = string.Format("{0}/Resources/Sounds", DBPathPlugin);


        internal static Texture2D texPanel = new Texture2D(16, 16, TextureFormat.ARGB32, false);
        internal static Texture2D texPanelSolarizedDark = new Texture2D(16, 16, TextureFormat.ARGB32, false);

        internal static Texture2D btnStartResearch = new Texture2D(17, 16, TextureFormat.ARGB32, false);
        internal static Texture2D btnPauseResearch = new Texture2D(17, 16, TextureFormat.ARGB32, false);
        internal static Texture2D btnStartAnalysis = new Texture2D(17, 16, TextureFormat.ARGB32, false);
        internal static Texture2D btnTransmitResults = new Texture2D(17, 16, TextureFormat.ARGB32, false);

        internal static Texture2D texPartWindowHead = new Texture2D(16, 16, TextureFormat.ARGB32, false);

        internal static Texture2D texBox = new Texture2D(9, 9, TextureFormat.ARGB32, false);
        internal static Texture2D texBoxUnity = new Texture2D(9, 9, TextureFormat.ARGB32, false);

        internal static Texture2D texSeparatorV = new Texture2D(6, 2, TextureFormat.ARGB32, false);
        internal static Texture2D texSeparatorH = new Texture2D(2, 20, TextureFormat.ARGB32, false);

        internal static void LoadTextures()
        {
            MonoBehaviourExtended.LogFormatted("Loading Textures");

            LoadImageFromFile(ref btnStartResearch, "MD-play.png", PathPluginResources);
            LoadImageFromFile(ref btnPauseResearch, "MD-pause.png", PathPluginResources);
            LoadImageFromFile(ref btnStartAnalysis, "search.png", PathPluginResources);
            LoadImageFromFile(ref btnTransmitResults, "connect.png", PathPluginResources);

            LoadImageFromFile(ref texPanel, "img_PanelBack.png");
            LoadImageFromFile(ref texPanelSolarizedDark, "img_PanelSolarizedDark.png");

            LoadImageFromFile(ref texPartWindowHead, "img_PartWindowHead.png");

            LoadImageFromFile(ref texBox, "tex_Box.png");
            LoadImageFromFile(ref texBoxUnity, "tex_BoxUnity.png");

            LoadImageFromFile(ref texSeparatorH, "img_SeparatorHorizontal.png");
            LoadImageFromFile(ref texSeparatorV, "img_SeparatorVertical.png");
        }


        #region Util Stuff
        //internal static Boolean LoadImageFromGameDB(ref Texture2D tex, String FileName, String FolderPath = "")
        //{
        //    Boolean blnReturn = false;
        //    try
        //    {
        //        //trim off the tga and png extensions
        //        if (FileName.ToLower().EndsWith(".png")) FileName = FileName.Substring(0, FileName.Length - 4);
        //        if (FileName.ToLower().EndsWith(".tga")) FileName = FileName.Substring(0, FileName.Length - 4); 
        //        //default folder
        //        if (FolderPath == "") FolderPath = DBPathTextures;

        //        //Look for case mismatches
        //        if (!GameDatabase.Instance.ExistsTexture(String.Format("{0}/{1}", FolderPath, FileName)))
        //            throw new Exception();
                
        //        //now load it
        //        tex = GameDatabase.Instance.GetTexture(String.Format("{0}/{1}", FolderPath, FileName), false);
        //        blnReturn = true;
        //    }
        //    catch (Exception)
        //    {
        //        MonoBehaviourExtended.LogFormatted("Failed to load (are you missing a file - and check case):{0}/{1}", FolderPath, FileName);
        //    }
        //    return blnReturn;
        //}

        /// <summary>
        /// Loads a texture from the file system directly
        /// </summary>
        /// <param name="tex">Unity Texture to Load</param>
        /// <param name="FileName">Image file name</param>
        /// <param name="FolderPath">Optional folder path of image</param>
        /// <returns></returns>
        public static Boolean LoadImageFromFile(ref Texture2D tex, String FileName, String FolderPath = "")
        {
            //DebugLogFormatted("{0},{1}",FileName, FolderPath);
            Boolean blnReturn = false;
            try
            {
                if (FolderPath == "") FolderPath = PathPluginTextures;

                //File Exists check
                if (System.IO.File.Exists(String.Format("{0}/{1}", FolderPath, FileName)))
                {
                    try
                    {
                        MonoBehaviourExtended.LogFormatted_DebugOnly("Loading: {0}", String.Format("{0}/{1}", FolderPath, FileName));
                        tex.LoadImage(System.IO.File.ReadAllBytes(String.Format("{0}/{1}", FolderPath, FileName)));
                        blnReturn = true;
                    }
                    catch (Exception ex)
                    {
                        MonoBehaviourExtended.LogFormatted("Failed to load the texture:{0} ({1})", String.Format("{0}/{1}", FolderPath, FileName), ex.Message);
                    }
                }
                else
                {
                    MonoBehaviourExtended.LogFormatted("Cannot find texture to load:{0}", String.Format("{0}/{1}", FolderPath, FileName));
                }


            }
            catch (Exception ex)
            {
                MonoBehaviourExtended.LogFormatted("Failed to load (are you missing a file):{0} ({1})", String.Format("{0}/{1}", FolderPath, FileName), ex.Message);
            }
            return blnReturn;
        }
        #endregion
    }
}
