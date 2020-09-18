# Easy.Admin入门

![ ](https://github.com/xxred/Easy.Admin/workflows/ASP.NET%20Core%20CI/badge.svg)
[![Easy.Admin](https://img.shields.io/nuget/vpre/Easy.Admin.svg?style=flat&label=Easy.Admin)](https://www.nuget.org/packages/Easy.Admin/)

案例演示：[NewLife.IdentityServer4](https://github.com/xxred/NewLife.IdentityServer4)，基于Easy.Admin开发，地址是 ids4.hebinghong.com

开发中...请勿直接用于生产

前端项目位于[Easy.Front-End](https://github.com/xxred/Easy.Front-End)

技术交流群：Easy.Admin->829687360，NewLife.XCode->1600800

## Easy.Admin 是什么

- 这是一套基于 aspnetcore 的通用权限框架，前后端分离方式。仅包含用户、角色、菜单这三个功能。包含日志、orm、缓存、api 文档生成以及常用开发小工具。
- Easy.Admin 提供了基础设施如：异常拦截、统一响应结果、自定义模型绑定等。除此之外的功能大部分由[NewLifex.XCode](https://github.com/NewLifeX/X)提供。

## 特点


- 让你具有快速开发的能力，特别适合业务不太复杂，但又有管理需求的系统。

- 简单且方便，无论是直接使用还是扣代码，要撘一个开发框架，里面总有你想要的。

- 比如实现一个单表的 curd，只需要两步，视图用的公共模板，也可覆盖替换：

-  [添加实体](https://github.com/xxred/IdentityServer4.XCode/blob/master/Entities/aIdentityServer.xml#L175)，执行 tt 文件生成实体

-  [添加控制器](https://github.com/xxred/NewLife.IdentityServer4/blob/master/NewLife.IdentityServer4/Controllers/ClientsController.cs)

  

## 起步
 

- 分别克隆前后端项目，注意是克隆而不是下载，否则会给后面带来麻烦。
 

```bash

git clone https://github.com/xxred/Easy.Admin.git

git clone https://github.com/xxred/Easy.Front-End.git

```

  

- 趁着克隆期间，下载前端环境，[nodejs](https://nodejs.org/en/)，安装完之后验证 node 版本以及安装 yarn。

  

```bash

node -v

npm -v

npm install -g yarn

```

  

- 项目下载完之后，进入前端项目执行命令`yarn`还原前端项目包。

- 运行后端项目，后端项目会自动运行前端项目，因此要保证前端项目路径配置正确。配置位于 appsettings.Development.json 的`ClientAppSourcePath`项，去掉此项则不运行前端项目，前端项目可单独跑。

- 如果后端项目运行时代码报错 IIS 没有启用，请点击带有绿色图标运行按钮内右边的箭头，下拉选择 Easy.Admin 再运行。如果后端项目已经运行，swagger 能访问，页面不能访问且报错包含`npm`，请检查前端项目路径配置是否正确，以及是否还原前端项目包

  

## 前置学习参考

  

- 后端大部分功能包含在 NewLife.XCode，特别是数据库操作部分，系列教程参考：https://www.cnblogs.com/nnhy/p/xcode_curd.html

- 前端部分教程参考：https://juejin.im/post/59097cd7a22b9d0065fb61d2

  

## 例程参考

  

- 上文提到的添加控制器和公共模板，参考[NewLife.IdentityServer4](https://github.com/xxred/NewLife.IdentityServer4)

- 公共页面模板以及模板替换参考，[NewLife.IdentityServer4.Vue](https://github.com/xxred/NewLife.IdentityServer4.Vue)

  
## 后端使用介绍

- 后端主要是用了 NewLife.XCode 作为数据库操作工具，系列教程[在此](https://www.cnblogs.com/nnhy/p/xcode_initdata.html#autoid-3-0-0)（想要知道怎么添加一个表吗？修改 xml 文件即可得到表对应实体，使劲戳它）。在此基础上，结合 aspnetcore，添加身份认证、异常拦截处理、swagger 文档、自动生成菜单、vue 开发中间件等基础功能。

## 功能概览

```csharp
        public void ConfigureServices(IServiceCollection services)
        {
            // 添加数据库连接
            services.AddConnectionStr();

            // 添加身份标识Identity
            services.AddIdentity(options =>
            {
                options.ClaimsIdentity.UserIdClaimType = JwtRegisteredClaimNames.Sub;
                options.ClaimsIdentity.UserNameClaimType = JwtRegisteredClaimNames.UniqueName;
            });

            // 添加身份验证
            services.ConfigAuthentication();

            services.AddMvc(options =>
            {
                options.ModelBinderProviders.Insert(0, new PagerModelBinderProvider());
                options.ModelBinderProviders.Insert(0, new EntityModelBinderProvider());
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
            .ConfigJsonOptions();

            // 文档
            services.ConfigSwaggerGen();

            // 跨域
            services.AddCors();

            // 扫描控制器添加菜单
            services.ScanController();
        }
```

1. 第一个是添加字符串链接，设置数据库连接字符串，格式其实自己定就可以了，只要把对应字符串设置上就行了，不用管格式到底是什么，怎么高兴怎么设置

   ```json
   {
     "connectionStrings": {
       "IdentityServer": {
         "connectionString": "Server=127.0.0.1;Port=3306;Database=IdentityServer;Uid=root;Pwd=123456;",
         "providerName": "MySql.Data.MySqlClient"
       },
       "Membership": {
         "connectionString": "Server=127.0.0.1;Port=3306;Database=IdentityServer;Uid=root;Pwd=123456;",
         "providerName": "MySql.Data.MySqlClient"
       }
     }
   }
   ```

2. 对应微软的 Identity 库，只不过用户类型修改为 x 组件的，登录注销等相关功能使用 UserManager，所以可以自由切换实体，使用任意 orm，其关键在于 IUserStore 和 IRoleStore 两个接口，在 IUserStore 的实现中处理系统与数据库交互

3. 给 MVC 添加了两个模型绑定器，一个用于处理分页，一个是处理实体。从请求中读取值并设置到相应模型，实体模型绑定器的工作是根据主键从数据库查询数据，然后赋值前端传过来的值。后面是 json 序列化配置，主要是命名规则设置、日期格式、序列化深度等

4. 添加 swagger 文档设置，根据配置显示文档标题、遍历项目目录添加项目注释文件、最后就是配合 OAuth2.0 登录了，避免每次登陆的繁琐

5. 跨域设置，可是设置 Policy，然后添加到控制器，这里为了测试方便，直接不写，在管道处设置全部放行

   ```csharp
   app.UseCors(options => { options.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().AllowCredentials(); });
   ```

6. 扫描控制器，生成菜单，将控制器上的权限操作码与菜单绑定，再设置到角色上，实现权限控制

7. 统一响应结果，ApiResultFilterAttribute 加在控制器基类，将所有响应结果包装成统一的格式

8. 异常拦截，请求过程中所有异常通过中间件 ApiExceptionMiddleware 拦截，返回统一的结果，以供前端展示友好结果

9. 集成第三方登录协议，内置了 QQ、Github 登录，微信的由于申请不到就没做。默认支持 OpenID Connect

### 控制器设计

- 首先是`AdminControllerBase`，该类是控制器基类。特性上对应功能分别是路由设置、统一结果封装、Api 控制器声明、身份认证过滤、跨域设置。成员有：当前当前请求对应登录用户、是否超级管理员、处理成功结果返回，处理失败结果返回

  ```csharp
    /// <summary>
    /// 基类Api
    /// </summary>
    [Route("api/[controller]")]
    [ApiResultFilter]
    [ApiController]
    [ApiAuthenticateFilter()]
    [EnableCors]
    public class AdminControllerBase : ControllerBase
    {

        private IUser _appUser;

        /// <summary>
        /// 当前用户
        /// </summary>
        public IUser AppUser
        {
            get => _appUser ?? (_appUser = HttpContext.Features.Get<IUser>());
            set => _appUser = value;
        }

        /// <summary>
        /// 是否超级管理员
        /// </summary>
        public bool IsSupperAdmin => AppUser.Role.IsSystem;

        /// <summary>
        /// 返回可带分页的结果
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="data"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        protected ApiResult Ok<TResult>(TResult data, PageParameter p = null)
        {
            return ApiResult.Ok(data, p);
        }

        /// <summary>
        /// 返回默认状态为402的结果
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        protected ApiResult Error(String msg = null, Int32 status = 402)
        {
            return ApiResult.Err(msg, status);
        }
    }
  ```

- 接着是实体`EntityController`，继承自`AdminControllerBase`，是个泛型类，只接受数据库实体。实体控制器包含列表搜索、单体查看、添加、更新、删除等基础功能，并用`ApiAuthorizeFilter`特性标记了权限。

  ```csharp
  /// <summary>
  /// 基类Api
  /// </summary>
  public class EntityController<TEntity> : AdminControllerBase where TEntity : Entity<TEntity>, new
  {
      /// <summary>
      /// 获取实体列表
      /// </summary>
      /// <param name="p">分页</param>
      /// <param name="key">搜索关键字</param>
      /// <returns></returns>
      [Route("Search")]
      [HttpPost]
      [ApiAuthorizeFilter(PermissionFlags.Detail)]
      [DisplayName("搜索{type}")]
      public virtual ApiResult<IList<TEntity>> Search([FromQuery]PageParameter p, [FromQuery]ring key){}

      /// <summary>
      /// 获取单对象
      /// </summary>
      /// <param name="id">对象id</param>
      /// <returns><see cref="T:TEntity" /></returns>
      [HttpGet("{id}")]
      [ApiAuthorizeFilter(PermissionFlags.Detail)]
      [DisplayName("查看{type}")]
      public virtual ApiResult<TEntity> Get([FromRoute]string id){}

      /// <summary>
      /// 添加
      /// </summary>
      /// <param name="value">需要添加的对象</param>
      [HttpPost]
      [ApiAuthorizeFilter(PermissionFlags.Insert)]
      [DisplayName("添加{type}")]
      public virtual ApiResult Post([FromBody]TEntity value){}

      /// <summary>
      /// 更新
      /// </summary>
      /// <param name="value">需要更新的对象</param>
      /// <returns></returns>
      [HttpPut]
      [ApiAuthorizeFilter(PermissionFlags.Update)]
      [DisplayName("更新{type}")]
      public virtual ApiResult Put([FromBody]TEntity value){}

      /// <summary>
      /// 删除
      /// </summary>
      /// <param name="id">需要删除对象的id</param>
      [HttpDelete("{id}")]
      [ApiAuthorizeFilter(PermissionFlags.Delete)]
      [DisplayName("删除{type}")]
      public virtual ApiResult Delete([FromRoute]string id){}

      /// <summary>
      /// 获取模型列信息
      /// </summary>
      /// <returns></returns>
      [HttpGet]
      [Route("GetColumns")]
      [ApiAuthorizeFilter(PermissionFlags.Detail)]
      [DisplayName("列信息{type}")]
      public virtual ApiResult<List<TableColumnDto>> GetColumns(){}
  }
  ```

### 使用

- 请确保完成了[NewLife.XCode](https://www.cnblogs.com/nnhy/p/xcode_initdata.html#autoid-3-0-0)系列教程
- 添加一个控制器很简单，新增实体之后，继承实体控制器就行，自带 curd，自动生成菜单，默认页面

  ```csharp
    /// <summary>
    /// 用户
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [DisplayName("用户")]
    public class UserController : EntityController<ApplicationUser>
    {
    }
  ```


## Easy.Admin 的权限管理

- 本节介绍 Easy.Admin 使用的权限管理功能的原理以及使用

### 原理

- 本着先实现再完善优化的原则，就不新造轮子了，而是直接使用 [NewLife.XCode](https://github.com/NewLifeX/X)自带的权限管理功能，下面详细介绍
- 从代码层面来讲，就是记录一个角色与一个控制器和控制器所有方法的关系。举个例子，管理员角色拥有用户控制器中添加用户、删除用户等方法的访问权限。假设一个控制器对应一个菜单，控制器的方法就是菜单的操作，这里用户菜单的 id 设为 1，添加用户这个操作标记为 1，删除用户操作标记为 2，更新用户操作标记为 4，依次类推标记所有操作为 2 的 n 次方
- 当然用什么标记可以自己定，这里只是让他们组成的列表符合位域的设计，即 2 的幂（即 1、2、4、8 等）。记录方式实际上就是将用户 id(这里设管理员 id 为 1)、菜单 id、操作标记、是否授权等这个几个属性记为一条数据。每次访问的时候，就根据当前用户和访问的菜单和操作，查询是否有授权，即可实现权限管理功能
- 比如，管理员角色对于用户菜单的添加、删除、更新操作，具有权限访问，这些数据记录为

  | 角色 id | 菜单 id | 操作 id | 是否授权 |
  | :-----: | :-----: | :-----: | :------: |
  |    1    |    1    |    1    |    是    |
  |    1    |    1    |    2    |    是    |
  |    1    |    1    |    4    |    是    |

- 当使用位域的方式记录所有操作时，比如同时授权添加、删除、更新这三个操作，那么就是`1+2+4 = 7`，二进制即`001 + 010 + 100 = 111`，也即`1|2|4 = 7`，每添加一个操作，直接用当前记录值和操作标记进行或运算。因为每个操作标记对应的二进制都是只有一个 1，而且位置不同，所以加起来不会产生进位，结果相当于累加(没有进位)。那么上述表记录变成
  | 角色 id | 菜单 id | 操作 id |
  | :-----: | :-----: | :-----: |
  | 1 | 1 | 7 |
- 那么怎么知道哪个操作被授权了呢？哪个位置上是 1 就说明哪个操作被授权。而判断方法就是与运算，需要判断的操作和记录值进行与运算，两个位置都是 1 的，结果对应位置才是 1，其余位置都是 0。比如，判断删除操作是否被授权，2 对应二进制 10，和记录值 7 的二进制 111 进行与运算，结果得 010。但如果，10 不在记录值里面，即 101，那么与运算结果为 0。`2 & 7 = 2 -> 10 & 111 = 10`，`10 & 101 = 0`
- 总的来说就是，添加操作就是或运算，判断操作就是与运算，那去掉操作呢？那也简单，实际上直接减去就行，对应的二进制运算叫做异或，也叫半加运算，没有进位的加法。比如`111 ⊕ 10`，10 加上去之后没有进位，结果是 101，相当于去掉了操作，大部分高级语言用的异或符号是`^`

### 使用

- 上面原理说起来也简单，说白了就是两个二进制操作，使用位域把记录简化。后来实际应用的时候，就将一个角色的所有菜单的操作权限全部合在一起，作为角色的一个字段，这个字段的值类似于`1#255,2#255,3#255,4#255`。设这个字段为`Permission`，逗号分隔每个菜单，每个记录是`菜单id#操作记录值`。上述值就是 1-4 这个几个菜单的操作值，都是 255，即`1|2|4|8|16|32|64|128`，默认可容纳 8 个操作
- 那么如何将控制器与菜单和操作关联起来呢？ [NewLife.XCode](https://github.com/NewLifeX/X)还提供了扫描控制器的代码，使用时通过在方法添加特性(注解)标记这个方法，代码就会通过反射将每个控制器生成一个菜单，方法生成对应菜单的操作，并记录标记值
- 权限判断的时候，也是通过这个特性(注解)获取当前方法的操作标记值，找到对应的菜单，再比较操作值
- 代码详见：
  - 菜单扫描：https://github.com/xxred/Easy.Admin/blob/master/Easy.Admin/Common/ScanController.cs#L22
  - 授权过滤：https://github.com/xxred/Easy.Admin/blob/master/Easy.Admin/Filters/ApiAuthorizeFilterAttribute.cs#L87

### 总结

- 对于一些复杂的权限设计，或者比较精细，那么自己写代码实现才是最好的选择，通用的权限设计毕竟只是满足大众需求
- 实用至上为原则，只要能很好解决你的问题，那它就是好的解决方案

