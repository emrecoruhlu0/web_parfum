import React from 'react';
import Sidebar from '../components/Sidebar';
import Feed from '../components/Feed';
import Widgets from '../components/Widgets';

export default function FeedPage() {
  return (
    <div className="min-h-screen bg-bg-light flex">
      <Sidebar />
      <div className="flex-1 ml-64 flex justify-center">
        <main className="max-w-[1200px] w-full flex px-6 py-8 gap-8">
          <div className="flex-1 max-w-[700px]">
            <Feed />
          </div>
          <div className="w-80 hidden lg:block sticky top-8 h-fit">
            <Widgets />
          </div>
        </main>
      </div>
    </div>
  );
}
