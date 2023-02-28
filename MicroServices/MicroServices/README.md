# 微服务的网关和服务注册中心

Ocelot.Provider.Consul 最后返回的是节点名称

Porvider 的获取服务代码：

```cs
if (IsValid(serviceEntry))
{
    QueryResult<Node[]> queryResult2 = await _consul.Catalog.Nodes();
    if (queryResult2.Response == null)
    {
        services.Add(BuildService(serviceEntry, null));
        continue;
    }

    Node serviceNode = queryResult2.Response.FirstOrDefault((Node n) => n.Address == serviceEntry.Service.Address);
    services.Add(BuildService(serviceEntry, serviceNode));
}
```

如果是再在本机上需要将电脑名称加入 hosts 文件中，不然访问不到对应 IP

同时需要修改对应 ASP.NET Core Web API 服务的绑定 URL，否则请求将会被拒绝
