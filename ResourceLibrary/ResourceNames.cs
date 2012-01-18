using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Exolutio.ResourceLibrary
{
    public class ExolutioResourceNames
    {
        public const string aggregate = "aggregate";
        public const string AlignObjectsBottom = "AlignObjectsBottom";
        public const string AlignObjectsCenteredHorizontal = "AlignObjectsCenteredHorizontal";
        public const string AlignObjectsCenteredVertical = "AlignObjectsCenteredVertical";
        public const string AlignObjectsLeft = "AlignObjectsLeft";
        public const string AlignObjectsRight = "AlignObjectsRight";
        public const string AlignObjectsTop = "AlignObjectsTop";
        public const string assocclass = "assocclass";
        public const string associate = "associate";
        public const string bilby = "bilby";
        public const string branch = "branch";
        public const string BringForward = "BringForward";
        public const string BringToFront = "BringToFront";
        public const string bullet_ball_red = "bullet_ball_red";
        public const string Camera = "Camera";
        public const string cancel = "cancel";
        public const string @class = "class";
        public const string comment = "comment";
        public const string compose = "compose";
        public const string Copy = "Copy";
        public const string cut = "cut";
        public const string delete = "delete";
        public const string delete2 = "delete2";
        public const string disk = "disk";
        public const string disks = "disks";
        public const string disk_blue = "disk_blue";
        public const string disk_multiple = "disk_multiple";
        public const string DistributeObjectsHorizontal = "DistributeObjectsHorizontal";
        public const string DistributeObjectsVertical = "DistributeObjectsVertical";
        public const string error_button = "error_button";
        public const string eye = "eye";
        public const string FolderDownload = "FolderDownload";
        public const string folder = "folder";
        public const string font = "font";
        public const string generalize = "generalize";
        public const string GenericDocument = "GenericDocument";
        public const string Group = "Group";
        public const string help = "help";
        public const string magnifier = "magnifier";
        public const string media_pause = "media_pause";
        public const string media_play_green = "media_play_green";
        public const string media_stop = "media_stop";
        public const string media_stop_red = "media_stop_red";
        public const string navigate_left = "navigate_left";
        public const string navigate_right = "navigate_right";
        public const string navigate_up = "navigate_up";
        public const string navigate_down = "navigate_down";
        public const string note_edit = "note_edit";
        public const string OpenFolder = "OpenFolder";
        public const string page = "page";
        public const string page_copy = "page_copy";
        public const string page_paste = "page_paste";
        public const string page_save = "page_save";
        public const string page_white = "page_white";
        public const string Paste = "Paste";
        public const string PasteBig = "PasteBig";
        public const string pencil = "pencil";
        public const string Print = "Print";
        public const string printer = "printer";
        public const string props = "props";
        public const string properties = "properties";
        public const string question_mark = "question_mark";
        public const string redo = "redo";
        public const string refresh = "refresh";
        public const string Save = "Save";
        public const string SendBackward = "SendBackward";
        public const string SendToBack = "SendToBack";
        public const string undo = "undo";
        public const string Ungroup = "Ungroup";
        public const string view_remove = "view_remove";
        public const string view_tree = "view_tree";
        public const string AttributeContainer = "AttributeContainer";
        public const string ContentContainer = "ContentContainer";
        public const string ContentChoice = "ContentChoice";
        public const string AddChildren = "AddChildren";
        public const string ClassUnion = "ClassUnion";
        public const string AddAttributes = "AddAttributes";
        public const string RemoveContainer = "RemoveContainer";
        public const string XmlSchema = "XmlSchema";
        public const string Validate = "Validate";
        public const string Warning = "Warning";
        public const string Palette = "Palette";
        public const string X = "X";
        public const string ExolutioIcon = "ExolutioIcon";
        public const string xmlIcon = "xmlIcon";
        public const string zoomIn = "zoomIn";
        public const string zoomOut = "zoomOut";
        public const string component_add = "component_add";
        public const string component_delete = "component_delete";
        public const string component_edit = "component_edit";
        public const string component_find = "component_find";
        public const string component_new = "component_new";
        public const string component_preferences = "component_preferences";
        public const string branch_delete = "branch_delete";
        public const string branch_element = "branch_element";
        public const string split_pim_assoc = "split_pim_assoc";
        public const string split_psm_assoc = "split_psm_assoc";

        public static ImageSource GetResourceImageSource(string resourceKey)
        {
            if (!Application.Current.Resources.Contains(resourceKey))
            {
                return null;
            }

            ImageSource resourceImageSource = Application.Current.Resources[resourceKey] as ImageSource;
            if (resourceImageSource  == null)
            {
                return null;    
            }
            else
            {
                return resourceImageSource;
            }
            
        }
    }
}