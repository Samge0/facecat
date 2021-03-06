/*基于捂脸猫FaceCat框架 v1.0
 捂脸猫创始人-矿洞程序员-脉脉KOL-陶德 (微信号:suade1984);
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using FaceCat;

namespace FaceCat
{
    /// <summary>
    /// 设计层
    /// </summary>
    public class DesignerDiv : FCTabControl
    {
        /// <summary>
        /// 创建设计层
        /// </summary>
        public DesignerDiv()
        {
            BorderColor = FCColor.None;
            m_parentVisibleChangedEvent = new FCEvent(parentVisibleChanged);
            Layout = FCTabPageLayout.Bottom;
        }

        /// <summary>
        /// 可以改变标题
        /// </summary>
        private bool m_canModifyCaption = true;

        /// <summary>
        /// 复制的控件
        /// </summary>
        private List<FCView> m_copys = new List<FCView>();

        /// <summary>
        /// 是否忽略下一次输入
        /// </summary>
        private bool m_ignoreNextInput;

        /// <summary>
        /// 是否是剪切
        /// </summary>
        private bool m_isCut = false;

        /// <summary>
        /// 父容器可见状态改变事件
        /// </summary>
        private FCEvent m_parentVisibleChangedEvent;

        /// <summary>
        /// 重做栈
        /// </summary>
        protected Stack<String> m_redoStack = new Stack<String>();

        /// <summary>
        /// 源代码是否改变
        /// </summary>
        private bool m_sourceCodeChanged = false;

        /// <summary>
        /// 撤销栈
        /// </summary>
        protected Stack<String> m_undoStack = new Stack<String>();

        private Designer m_designer;

        /// <summary>
        /// 获取或设置设计器
        /// </summary>
        public Designer Designer
        {
            get { return m_designer; }
            set { m_designer = value; }
        }

        private FCTabPage m_designerTabPage;

        /// <summary>
        /// 获取或设置设计页
        /// </summary>
        public FCTabPage DesignerTabPage
        {
            get { return m_designerTabPage; }
            set { m_designerTabPage = value; }
        }

        /// <summary>
        /// 获取或设置是否已修改
        /// </summary>
        public bool Modified
        {
            get 
            {
                if (m_canModifyCaption)
                {
                    FCTabPage tabPage = Parent as FCTabPage;
                    if (tabPage != null)
                    {
                        String caption = tabPage.Text;
                        if (caption.EndsWith("*"))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            set
            {
                if (m_canModifyCaption)
                {
                    FCTabPage tabPage = Parent as FCTabPage;
                    if (tabPage != null)
                    {
                        String caption = tabPage.Text;
                        if (value)
                        {
                            if (!caption.EndsWith("*"))
                            {
                                caption += "*";
                            }
                        }
                        else
                        {
                            if (caption.EndsWith("*"))
                            {
                                caption = caption.Substring(0, caption.Length - 1);
                            }
                        }
                        tabPage.Text = caption;
                        tabPage.HeaderButton.invalidate();
                    }
                }
            }
        }

        private ResizeDiv m_resizeDiv;

        /// <summary>
        /// 获取或设置调整尺寸的层
        /// </summary>
        public ResizeDiv ResizeDiv
        {
            get { return m_resizeDiv; }
            set { m_resizeDiv = value; }
        }

        private FCTabPage m_sourceCodeTabPage;

        /// <summary>
        /// 获取或设置源代码页
        /// </summary>
        public FCTabPage SourceCodeTabPage
        {
            get { return m_sourceCodeTabPage; }
            set { m_sourceCodeTabPage = value; }
        }

        private ScintillaX m_scintilla;

        /// <summary>
        /// 获取或设置文本编辑器
        /// </summary>
        public ScintillaX Scintilla
        {
            get { return m_scintilla; }
            set { m_scintilla = value; }
        }

        private UIXmlEx m_xml;

        /// <summary>
        /// 获取或设置XML对象
        /// </summary>
        public UIXmlEx Xml
        {
            get { return m_xml; }
            set { m_xml = value; }
        }

        private String m_xmlPath;

        /// <summary>
        /// 获取XML的路径
        /// </summary>
        public String XmlPath
        {
            get { return m_xmlPath; }
        }

        /// <summary>
        /// 判断是否可以剪贴
        /// </summary>
        /// <returns>状态</returns>
        public bool canCut()
        {
            if (m_sourceCodeTabPage.Visible)
            {
                return m_scintilla.Selection.Length > 0;
            }
            else
            {
                return m_resizeDiv.getTargets().Count > 0;
            }
        }

        /// <summary>
        /// 判断是否可以复制
        /// </summary>
        /// <returns>状态</returns>
        public bool canCopy()
        {
            if (m_sourceCodeTabPage.Visible)
            {
                return true;
            }
            else
            {
                return m_resizeDiv.getTargets().Count > 0;
            }
        }

        /// <summary>
        /// 判断是否可以删除
        /// </summary>
        /// <returns></returns>
        public bool canDelete()
        {
            if (m_sourceCodeTabPage.Visible)
            {
                return m_scintilla.Selection.Length > 0;
            }
            else
            {
                return m_resizeDiv.getTargets().Count > 0;
            }
        }

        /// <summary>
        /// 判断是否可以粘贴
        /// </summary>
        /// <returns></returns>
        public bool canPaste()
        {
            if (m_sourceCodeTabPage.Visible)
            {
                return m_scintilla.Clipboard.CanPaste; 
            }
            else
            {
                return m_copys.Count > 0;
            }
        }

        /// <summary>
        /// 判断是否可以重复
        /// </summary>
        /// <returns>是否可以重复</returns>
        public bool canRedo()
        {
            if (m_sourceCodeTabPage.Visible)
            {
                return m_scintilla.NativeInterface.CanRedo();
            }
            else
            {
                return m_redoStack.Count > 0;
            }
        }

        /// <summary>
        /// 判断是否可以撤销
        /// </summary>
        /// <returns>是否可以撤销</returns>
        public bool canUndo()
        {
            if (m_sourceCodeTabPage.Visible)
            {
                return m_scintilla.NativeInterface.CanUndo();
            }
            else
            {
                return m_undoStack.Count > 0;
            }
        }

        /// <summary>
        /// 重置
        /// </summary>
        public void ClearRedoUndo()
        {
            m_undoStack.Clear();
            m_redoStack.Clear();
        }

        /// <summary>
        /// 复制
        /// </summary>
        public void copy()
        {
            m_isCut = false;
            if (m_sourceCodeTabPage.Visible)
            {
                m_scintilla.Clipboard.Copy();
            }
            else
            {
                m_copys.Clear();
                List<FCView> targets = m_resizeDiv.getTargets();
                int targetsSize = targets.Count;
                for (int i = 0; i < targetsSize; i++)
                {
                    m_copys.Add(targets[i]);
                }
            }
        }

        /// <summary>
        /// 剪切
        /// </summary>
        public void cut()
        {
            m_isCut = true;
            if (m_sourceCodeTabPage.Visible)
            {
                m_scintilla.Clipboard.Cut();
            }
            else
            {
                m_copys.Clear();
                List<FCView> targets = m_resizeDiv.getTargets();
                int targetsSize = targets.Count;
                for (int i = 0; i < targetsSize; i++)
                {
                    m_copys.Add(targets[i]);
                }
            }
        }


        /// <summary>
        /// 删除
        /// </summary>
        public void del()
        {
            m_isCut = true;
            if (m_designerTabPage.Visible)
            {
                m_copys.Clear();
                UIXmlEx xml = m_resizeDiv.Xml;
                List<FCView> targets = m_resizeDiv.getTargets();
                int targetsSize = targets.Count;
                for (int i = 0; i < targetsSize; i++)
                {
                    xml.removeControl(targets[i]);
                }
                m_resizeDiv.clearTargets();
                m_resizeDiv.Visible = false;
                m_native.invalidate();
            }
        }


        /// <summary>
        /// 销毁控件方法
        /// </summary>
        public override void delete()
        {
            if (m_scintilla != null)
            {
                FCNative native = Native;
                WinHostEx host = native.Host as WinHostEx;
                Control container = Control.FromHandle(host.HWnd);
                if (container != null)
                {
                    container.Controls.Remove(m_scintilla);
                    m_scintilla.Dispose();
                    m_scintilla = null;
                }
            }
            if (m_xml != null)
            {
                m_xml.delete();
                m_xml = null;
            }
            base.delete();
        }

        /// <summary>
        /// 文档改变事件
        /// </summary>
        /// <param name="sender">调用者</param>
        private void documentChanged(object sender)
        {
            Modified = true;
        }

        /// <summary>
        /// 获取编辑页偏移
        /// </summary>
        /// <returns>坐标</returns>
        public FCPoint getDesignerOffset()
        {
            FCNative native= Native;
            int clientX = native.clientX(m_designerTabPage);
            int clientY = native.clientY(m_designerTabPage);
            clientX -= (m_designerTabPage.HScrollBar != null ? m_designerTabPage.HScrollBar.Pos : 0);
            clientY -= (m_designerTabPage.VScrollBar != null ? m_designerTabPage.VScrollBar.Pos : 0);
            return new FCPoint(clientX, clientY);
        }

        /// <summary>
        /// 添加控件方法
        /// </summary>
        public override void onLoad()
        {
            base.onLoad();
            if (m_designerTabPage == null)
            {
                m_designerTabPage = new FCTabPage();
                addControl(m_designerTabPage);
                m_designerTabPage.BorderColor = FCColor.None;
                m_designerTabPage.HeaderButton.Size = new FCSize(60, 20);
                m_designerTabPage.HeaderButton.Margin = new FCPadding(1, -1, 0, 2);
                m_designerTabPage.ShowHScrollBar = true;
                m_designerTabPage.ShowVScrollBar = true;
                m_designerTabPage.Text = "设计";
                m_designerTabPage.BackColor = FCColor.argb(75, 51, 153, 255);
                m_resizeDiv = new ResizeDiv();
                m_resizeDiv.Native = Native;
                m_designerTabPage.addControl(m_resizeDiv);
            }
            if (m_sourceCodeTabPage == null)
            {
                m_sourceCodeTabPage = new FCTabPage();
                m_sourceCodeTabPage.BorderColor = FCColor.None;
                //创建编辑器
                m_scintilla = new ScintillaX();
                m_scintilla.Visible = false;
                FCNative native = Native;
                WinHostEx host = native.Host as WinHostEx;
                Control container = Control.FromHandle(host.HWnd);
                container.Controls.Add(m_scintilla);
                m_scintilla.ParentDiv = m_sourceCodeTabPage;
                m_scintilla.TextChanged += new EventHandler<EventArgs>(scintilla_TextChanged);
                addControl(m_sourceCodeTabPage);
                m_sourceCodeTabPage.Text = "源";
                m_sourceCodeTabPage.HeaderButton.Size = new FCSize(60, 20);
                m_sourceCodeTabPage.HeaderButton.Margin = new FCPadding(1, -1, 0, 2);
            }
            if (Parent != null)
            {
                Parent.addEvent(m_parentVisibleChangedEvent, FCEventID.VISIBLECHANGED);
            }
            SelectedIndex = 0;
        }

        /// <summary>
        /// 页选中改变方法
        /// </summary>
        public override void onSelectedTabPageChanged()
        {
            base.onSelectedTabPageChanged();
            if (m_xml != null)
            {
                FCNative native = Native;
                FCTabPage selectedTabPage = SelectedTabPage;
                if (selectedTabPage != null)
                {
                    bool modified = Modified;
                    m_canModifyCaption = false;
                    //设计视图
                    if (selectedTabPage == m_designerTabPage)
                    {
                        if (m_sourceCodeChanged)
                        {                     
                            if (m_scintilla != null)
                            {
                                String xml = m_scintilla.Text;
                                String error = "";
                                if (m_xml.checkXml(xml, ref error))
                                {
                                    saveUndo();
                                    openXml(xml);
                                    m_sourceCodeChanged = false;
                                }
                            }
                        }
                    }
                    //源代码
                    else if (selectedTabPage == m_sourceCodeTabPage)
                    {
                        if (m_scintilla != null)
                        {
                            String xml = m_scintilla.Text;
                            if (xml.Length == 0 || modified)
                            {
                                String error = "";
                                if (xml.Length == 0 || m_xml.checkXml(xml, ref error))
                                {
                                    m_scintilla.loadXml(m_xml.DocumentText.Replace("﻿<", "<"));
                                    m_ignoreNextInput = true;
                                }
                            }
                            m_designer.viewSource();
                        }
                    }
                    m_canModifyCaption = true;
                }
            }
        }

        /// <summary>
        /// 打开文件
        /// </summary>
        /// <param name="fileName">文件名</param>
        public void openFile(String fileName)
        {
            if (m_xml == null)
            {
                m_xml = new UIXmlEx();
                m_xml.Native = Native;
                m_xml.DocumentChangedEvents.Add(new FCEvent(documentChanged));
                FileInfo fileInfo = new FileInfo(fileName);
                if (fileInfo != null)
                {
                    if (fileInfo.Directory != null)
                    {
                        DesignerTabPage.ResourcePath = fileInfo.Directory.FullName;
                    }
                }
            }
            m_canModifyCaption = false;
            openXml(File.ReadAllText(fileName));
            m_xmlPath = fileName;
            m_canModifyCaption = true;
            saveUndo();
        }

        /// <summary>
        /// 打开XML
        /// </summary>
        /// <param name="xml">XML</param>
        public void openXml(String xml)
        {
            //获取选中控件的名称
            List<FCView> targets = m_resizeDiv.getTargets();
            int targetsSize = targets.Count;
            List<String> names = new List<String>();
            for (int i = 0; i < targetsSize; i++)
            {
                names.Add(targets[i].Name);
            }
            int namesSize = names.Count;
            //移动控件
            List<FCView> controls = m_designerTabPage.getControls();
            int controlsSize = controls.Count;
            for (int i = 0; i < controlsSize; i++)
            {
                FCView control = controls[i];
                if (control != m_resizeDiv)
                {
                    m_designerTabPage.removeControl(control);
                    controlsSize--;
                    i--;
                }
            }
            targets.Clear();
            m_resizeDiv.clearTargets();
            //加载XML
            m_xml.loadXml(xml, m_designerTabPage);
            if (namesSize > 0)
            {
                for (int i = 0; i < namesSize; i++)
                {
                    FCView target = m_xml.findControl(names[i]);
                    if (target != null)
                    {
                        targets.Add(target);
                    }
                }
                if (targets.Count > 0)
                {
                    m_designer.setCurrentControl(targets[0]);
                }
                targets.Clear();
            }
            else
            {
                m_resizeDiv.Visible = false;
            }
            m_designerTabPage.update();
            invalidate();
        }

        /// <summary>
        /// 父容器可见状态改变事件
        /// </summary>
        /// <param name="sender">调用者</param>
        private void parentVisibleChanged(object sender)
        {
            if (m_scintilla != null)
            {
                FCView parent = Parent;
                if (parent.Visible)
                {
                    if (m_sourceCodeTabPage != null && m_sourceCodeTabPage.Visible)
                    {
                        m_scintilla.Visible = true;
                    }
                }
                else
                {
                    m_scintilla.Visible = false;
                }
            }
        }

         /// <summary>
        /// 粘贴
        /// </summary>
        public void paste()
        {
            if (m_sourceCodeTabPage.Visible)
            {
                m_scintilla.Clipboard.Paste();
            }
            else
            {
                int copysSize = m_copys.Count;
                if (copysSize > 0)
                {
                    UIXmlEx targetXml = Xml;
                    FCPoint mp = m_native.TouchPoint;
                    //查找控件
                    List<FCView> targets = m_resizeDiv.getTargets();
                    int targetsSize = targets.Count;
                    if (targetsSize > 0)
                    {
                        FCView targetControl = targets[0];
                        if (!targetXml.isContainer(targetControl))
                        {
                            targetControl = targetControl.Parent;
                        }
                        if (targetControl != null && targetXml.containsControl(targetControl))
                        {
                            for (int i = 0; i < copysSize; i++)
                            {
                                FCView control = m_copys[i];
                                if (m_xml.containsControl(control))
                                {
                                    targetXml.copyControl(UITemplate.CreateControlName(control, m_xml), control, targetControl);
                                    if (m_isCut)
                                    {
                                        m_xml.removeControl(control);
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            m_isCut = false;
            m_copys.Clear();
            m_native.invalidate();
        }

        /// <summary>
        /// 重复
        /// </summary>
        /// <returns>重复命令</returns>
        public void redo()
        {
            if (canRedo())
            {
                if (m_designerTabPage.Visible)
                {
                    m_undoStack.Push(m_xml.XmlDoc.InnerXml);
                    openXml(m_redoStack.Pop());
                }
                else
                {
                    m_scintilla.NativeInterface.Redo();
                }
            }
        }

        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="fileName">文件名</param>
        public void saveFile(String fileName)
        {
            FCTabPage selectedTabPage = SelectedTabPage;
            if (selectedTabPage != null)
            {
                if (selectedTabPage == m_sourceCodeTabPage)
                {
                    if (m_sourceCodeChanged)
                    {                    
                        String xml = m_scintilla.Text;
                        String error = "";
                        if (m_xml.checkXml(xml, ref error))
                        {
                            openXml(xml);
                        }
                        else
                        {
                            MessageBox.Show(error, "提示");
                            return;
                        }
                        m_sourceCodeChanged = false;
                    }
                }
                m_xml.saveFile(fileName);
                if (selectedTabPage != m_sourceCodeTabPage)
                {
                    m_scintilla.loadXml(m_xml.DocumentText.Replace("﻿<", "<"));
                    m_ignoreNextInput = true;
                }
                Modified = false;
            }
        }

        /// <summary>
        /// 保存撤销
        /// </summary>
        /// <param name="cmd">命令</param>
        public void saveUndo()
        {
            String oldXml = "";
            if (m_undoStack.Count > 0)
            {
                oldXml = m_undoStack.Peek();
            }
            String newXml = m_xml.XmlDoc.InnerXml;
            if (oldXml != newXml)
            {
                m_undoStack.Push(newXml);
            }
        }

        /// <summary>
        /// 编辑器文字改变事件
        /// </summary>
        /// <param name="sender">调用者</param>
        /// <param name="e">参数</param>
        private void scintilla_TextChanged(object sender, EventArgs e)
        {
            if (!m_ignoreNextInput)
            {
                m_sourceCodeChanged = true;
                Modified = true;
            }
            m_ignoreNextInput = false;
        }

        /// <summary>
        /// 撤销
        /// </summary>
        /// <returns>撤销命令</returns>
        public void undo()
        {
            if (canUndo())
            {
                if (m_designerTabPage.Visible)
                {
                    m_redoStack.Push(m_xml.XmlDoc.InnerXml);
                    openXml(m_undoStack.Pop());
                }
                else
                {
                    m_scintilla.NativeInterface.Undo();
                }
            }
        }
    }
}
