# SE Script - Visual Information System

#### Configuration

**Im Programmierbaren Block**

* Display Name Tag:
  * Syntax: displaytag:name
  * Beschreibung: Legt fest unter welchem Tag ein Bildschirm gefunden werden kann, damit dieses Script es benutzen kann. Der Standardwert ist "[VIS]".

* Vorlage
  * Syntax: template:name
  * Beschreibung: Beginnt ein neues Template das im späteren Verlauf der Konfiguration genutzt werden kann.
 
**Im jeweiligen Bildschirm**

* display
  * Syntax: display:id:group:x,y
  * Beschreibung: Configure this display to use with the VIS. A Display can be a single lcd or a group of lcd panels. It defines more a virtual display that inerhite at least one render target. If you want to setup multiple render targets you need to set a group. In this case all displays passing his rendert targets to one display object.
   * id:    Is set as an integer and is the Surface ID of a block.
   * group: The name of your group. This parameter is optional. So, you need to set this parameter only if you want to group multiple displays.
   * x,y:   These are the coorinate of a display inside the group. Values are given as integer values. The top left corner has the coorindate 0,0 and the bottem right n,m. You need to set it if you setup a group.
   * Hinweis: All further configurations will be read only at display with the coordinate 0,0.
 
                 /*!
                 * Set the name of an existing template. The name is declared as a string. If you set
                 * a template all values will be copied from the existing one. Every data below this
                 * statement overrides his individual value. So if you want to modify an existing
                 * template declare this statement first and all other below.
                 * 
                 * Syntax: template:name
                 */
                 
                /*!
                 * Set the interval in seconds in which this display will update it's content.
                 * The value is a floating point value. A value of 1.0 means one second. The
                 * default value is 5s.
                 * 
                 * Syntax: updateinterval:value
                 */
                 
                /*!
                 * Setting the background color. Colors are always rgb integer values between
                 * 0 ... 255. The alpha value is optional. The default value is black.
                 * 
                 * Syntax: backgroundcolor:r,g,b(,a)
                 */
                 
                /*!
                 * Setting the default font with size and color. The size is a floating point value.
                 * Colors are always rgb integer values between 0 ... 255. The alpha value is 
                 * optional. The values of size and color are optional. If you don't set this values 
                 * size will be set to 1.0f and color to white.
                 * 
                 * Syntax: font:name:size:r,g,b(,a)
                 */
                 
                /*!
                 * Setting up a single graphic. A graphic can be a simple text, a simple quad or
                 * a complex structure like a bar. Set the name to select which type of graphic
                 * you want to use. The options will be passed through the graphic.
                 * 
                 * Syntax: graphic:type:dataretriever:(options)
                 */
                 
**Eine neue Grafik beginnen**

Dies ist Teil einer Vorlage oder direkt teil der Konfiugration eines Bildschirms.

                /*!
                 * Set a color. This color is used for all sprites. The alpha value is
                 * optional.
                 * 
                 * Syntax: color:r,g,b(,a)
                 */
                 
                /*!
                 * Set a color for a gradient. This colors are used if a graphic has a data collector. The alpha
                 * value is optional. The indicator is set as floating point value. The interpretation is
                 * from 0 to n
                 * 
                 * Syntax: gradient:indicator:r,g,b(,a)
                 */
                 
                /*!
                 * Syntax: check:type:options
                 */

**Graphic:Text Konfiguration**

            /*!
             * Setting the font with size and color. The size is a floating point value. Colors
             * are always rgb integer values between 0 ... 255. The alpha value is optional. The
             * values of size and color are optional. If you don't set this values size will be
             * set to 0.0f and color to white. If size is zero the font will scale by size.
             * 
             * Syntax: font:name:size:r,g,b(,a)
             */
             
            /*!
             * Set the text that you want to display.
             * 
             * Syntax: text:string
             */
             
            /*!
             * Set the alignment of a text.
             * 
             * Syntax: alignment:c|center|l|left|r|right
             */
