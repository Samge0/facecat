/*捂脸猫FaceCat框架 v1.0
 1.创始人-矿洞程序员-上海宁米科技创始人-脉脉KOL-陶德 (微信号:suade1984);
 2.联合创始人-上海宁米科技创始人-袁立涛(微信号:wx627378127);
 3.联合创始人-河北思尔企业管理咨询有限公司合伙人-肖添龙(微信号:xiaotianlong_luu);
 4.联合开发者-陈晓阳(微信号:chenxiaoyangzxy)，助理-朱炜(微信号:cnnic_zhu);
 5.该框架开源协议为BSD，欢迎对我们的创业活动进行各种支持，欢迎更多开发者加入。
 包含C/C++,Java,C#,iOS,MacOS,Linux六个版本的图形和通讯服务框架。
 */
package facecat.topin.core;

/*
* 坐标点
*/
public class FCPoint {

    /*
    * 创建点
    */
    public FCPoint() {
    }

    /**
     * 创建点
     *
     * @param x 横坐标
     * @param y 纵坐标
     */
    public FCPoint(int x, int y) {
        this.x = x;
        this.y = y;
    }

    /**
     * 创建点
     *
     * @param x 横坐标
     * @param y 纵坐标
     */
    public FCPoint(float x, float y) {
        this.x = (int) x;
        this.y = (int) y;
    }

    /**
     * 横坐标
     */
    public int x;

    /**
     * 纵坐标
     */
    public int y;

    /*
    * 拷贝数据
    */
    public FCPoint clone() {
        FCPoint varCopy = new FCPoint();

        varCopy.x = this.x;
        varCopy.y = this.y;

        return varCopy;
    }
}
