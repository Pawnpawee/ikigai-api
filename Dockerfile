#? Stage 1: Build Stage - ใช้ Image SDK ตัวเต็มเพื่อ Compile และ Build โปรเจค
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

//* Copy ไฟล์ .csproj ของทุก Layer มาก่อน เพื่อใช้ฟีเจอร์ Docker Layer Caching
//* การทำแบบนี้จะทำให้การ Build ครั้งต่อๆ ไปเร็วขึ้นมาก ถ้าเราไม่ได้แก้ dependencies
COPY ["ikigai-api.API/ikigai-api.API.csproj", "ikigai-api.API/"]
COPY ["ikigai-api.Application/ikigai-api.Application.csproj", "ikigai-api.Application/"]
COPY ["ikigai-api.Domain/ikigai-api.Domain.csproj", "ikigai-api.Domain/"]
COPY ["ikigai-api.Infrastructure/ikigai-api.Infrastructure.csproj", "ikigai-api.Infrastructure/"]

//* Restore dependencies ทั้งหมด
RUN dotnet restore "ikigai-api.API/ikigai-api.API.csproj"

//* Copy source code ทั้งหมดที่เหลือ
COPY . .

//* Build โปรเจคโดยเน้น Configuration แบบ Release เพื่อประสิทธิภาพสูงสุด
WORKDIR "/src/ikigai-api.API"
RUN dotnet build "ikigai-api.API.csproj" -c Release -o /app/build

#? Stage 2: Publish Stage - เตรียมไฟล์สำหรับนำไปใช้งานจริง
FROM build AS publish
RUN dotnet publish "ikigai-api.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

#? Stage 3: Runtime Stage - ใช้ Image ตัวเล็ก (Alpine หรือ Distroless) เพื่อความปลอดภัยและขนาดที่เล็กที่สุด
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

//! warning bug need to fix: อย่า Hardcode PORT ใน Code ให้รับค่าจาก Environment Variable ของ Railway
#? Railway จะส่ง PORT มาให้ Environment Variable ชื่อ "PORT" โดยอัตโนมัติ
#? .NET 8 สามารถรับค่านี้ได้เลยผ่าน ASPNETCORE_HTTP_PORTS (เราจะไปตั้งค่าใน Railway อีกที)

ENTRYPOINT ["dotnet", "ikigai-api.API.dll"]